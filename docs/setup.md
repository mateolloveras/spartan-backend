# Módulo Setup — Solución .NET base (T-001)

## Descripción

Ticket T-001 completa la infraestructura base de la solución `spartan-backend`: cuatro capas de Clean Architecture, pipeline transversal de validación y manejo de errores, convenciones HTTP unificadas y OpenAPI en desarrollo.

No contiene lógica de negocio; es la plataforma que todos los tickets posteriores (T-002, T-006, módulos) heredan y usan.

---

## Arquitectura

### Capas

```
Domain
  ↑
Application
  ↑
Infrastructure → (solo para registrar servicios)
Presentation ← (composition root)
```

**Domain (`SpartanGym.Domain`)**
- Excepciones de dominio: `NotFoundException`, `ValidationException`, `DomainException`, `UnauthorizedException`.
- Sin dependencias externas (ni EF, ni ASP.NET, ni MediatR, ni FluentValidation).

**Application (`SpartanGym.Application`)**
- MediatR 12.5.0: CommandQuery Separation Pattern.
- Pipeline de validación: `ValidationBehavior<TRequest, TResponse>` intercepta todo request y valida antes del Handler.
- FluentValidation para especificar reglas de cada Command y Query.
- `PagedResult<T>`: DTO para respuestas paginadas.
- Sin dependencia a Infrastructure.

**Infrastructure (`SpartanGym.Infrastructure`)**
- Servicios de persistencia, caché, APIs externas, etc.
- Registrados en `DependencyInjection.AddInfrastructure(...)`.
- En T-001, vacío; T-002 agrega `DbContext`, `IUnitOfWork` y repositorios.

**Presentation (`SpartanGym.Presentation`)**
- ASP.NET Core Web API.
- Controllers y Minimal APIs.
- Middleware global: `DomainExceptionHandler`, `ProblemDetailsDefaults`.
- Convenciones JSON y serialización.
- OpenAPI setup.
- `Program.cs` es el composition root; registra todas las capas y configura el pipeline HTTP.

### Pipeline HTTP

En `Program.cs`, este orden (crítico):

1. **`UsePathBase("/api")`** — prefijo global; todas las rutas se sirven bajo `/api`.
2. **`UseExceptionHandler()`** — captura excepciones y las delega a `DomainExceptionHandler`.
3. **`UseStatusCodePages()`** — envuelve códigos HTTP 401, 403, 404, 500 en `ProblemDetails` customizados.
4. **`UseCors("Angular")`** — habilita CORS desde `http://localhost:4200`.
5. **`UseAuthentication()`** — (vacío hasta T-006, solo ordena el pipeline).
6. **`UseAuthorization()`** — (vacío hasta T-006, solo ordena el pipeline).
7. **`MapControllers()`** — enruta a Controllers.
8. **`MapOpenApi()`** — en Development: expone OpenAPI en `/api/openapi/v1.json`.

### Excepciones y errores

```
Validación de FluentValidation
  → ValidationBehavior (Application)
    → Handler procesa lógica
      → puede lanzar DomainException
Excepciones capturadas
  → DomainExceptionHandler (Presentation)
    → HTTP Status 400, 401, 403, 404, 409 o 500
    → ProblemDetails JSON en snake_case
```

**Mapeo:**
| Excepción | Status HTTP |
|---|---|
| `FluentValidation.ValidationException` | 400 con `errors` |
| `SpartanGym.Domain.ValidationException` | 400 sin `errors` |
| `SpartanGym.Domain.UnauthorizedException` | 401 |
| `SpartanGym.Domain.NotFoundException` | 404 |
| `SpartanGym.Domain.DomainException` | 409 |
| Otra excepción inesperada | 500 |

---

## Estructura de carpetas

```
spartan-backend/
├── .gitignore                   ← actualizado: +TestResults/, *.coverage
├── CLAUDE.md                    ← desactualizado (describe monolito viejo)
├── Directory.Build.props        ← centralización de propiedades (TFM, nullable)
├── Directory.Packages.props     ← versiones de todos los NuGet
├── SpartanGym.sln              ← solución con 5 proyectos
├── docker-compose.yml          ← PostgreSQL 17 (sin cambios)
├── docs/
│   ├── setup.md                ← este archivo
│   └── contracts/
│       └── setup.md            ← contrato HTTP definitivo
├── src/
│   ├── SpartanGym.Domain/
│   │   ├── SpartanGym.Domain.csproj
│   │   └── Exceptions/
│   │       ├── DomainException.cs
│   │       ├── NotFoundException.cs
│   │       ├── UnauthorizedException.cs
│   │       └── ValidationException.cs
│   ├── SpartanGym.Application/
│   │   ├── SpartanGym.Application.csproj
│   │   ├── DependencyInjection.cs
│   │   └── Common/
│   │       ├── Behaviors/
│   │       │   └── ValidationBehavior.cs
│   │       └── PagedResult.cs
│   ├── SpartanGym.Infrastructure/
│   │   ├── SpartanGym.Infrastructure.csproj
│   │   └── DependencyInjection.cs
│   └── SpartanGym.Presentation/
│       ├── SpartanGym.Presentation.csproj
│       ├── Program.cs
│       ├── DependencyInjection.cs
│       ├── Properties/launchSettings.json
│       ├── appsettings.json
│       ├── Controllers/
│       │   └── HealthController.cs
│       ├── Middleware/
│       │   ├── DomainExceptionHandler.cs
│       │   └── ProblemDetailsDefaults.cs
│       └── Serialization/
│           └── JsonConventions.cs
└── tests/
    └── SpartanGym.Tests/
        ├── SpartanGym.Tests.csproj
        ├── Application/
        │   └── ValidationBehaviorTests.cs
        ├── Presentation/
        │   ├── DomainExceptionHandlerTests.cs
        │   └── PipelineTests.cs
        ├── Architecture/
        │   └── LayerDependencyTests.cs
        └── Support/
            ├── SpartanWebApplicationFactory.cs
            ├── ProbeController.cs
            ├── ProbeCommand.cs
            └── TestAuthHandler.cs
```

---

## Cómo compilar y ejecutar

### Prerequisites

- .NET 10 SDK instalado.
- PostgreSQL 17 via Docker o instalado localmente.
- git.

### Compilar

```bash
cd spartan-backend
dotnet build SpartanGym.sln
```

Sin warnings ni errores.

### Tests

```bash
dotnet test SpartanGym.sln
```

Deberían pasar 27 tests (unitarios e integración).

### Ejecutar la aplicación

```bash
# Opción 1: desde Visual Studio / VS Code
dotnet run --project src/SpartanGym.Presentation

# Opción 2: con el perfil de launch explícito
dotnet run --project src/SpartanGym.Presentation --launch-profile http
```

La API arranca en `http://localhost:5000` (puerto por defecto en launchSettings).

### Verificar que funciona

```bash
# Health check
curl -s http://localhost:5000/api/health | jq .
# Respuesta: { "status": "healthy" }

# OpenAPI (solo Development)
curl -s http://localhost:5000/api/openapi/v1.json | jq . | head -20
```

---

## Convenciones y pautas

### Nombres

- **Proyectos/namespaces:** `SpartanGym.*` (no `SpartanBackend`).
- **JSON (snake_case):** propiedades y enums. Ejemplo:
  ```csharp
  public record CreateUserRequest(string UserName, string Email);
  // JSON: { "user_name": "...", "email": "..." }
  
  public enum UserStatus { Active, Inactive }
  // JSON: "active", "inactive"
  ```

### Validators y Handlers

- **Ubicación:** `Application/Features/{ModuleName}/{CommandOrQueryName}Validator.cs` y `.Handler.cs`.
- **FluentValidation:** Cada Command y Query tiene su propio `Validator<TRequest>` en `Application`.
- **MediatR:** Cada operación tiene su `IRequestHandler<TRequest, TResponse>` que la implementa.
- **Ejemplo:**
  ```csharp
  namespace SpartanGym.Application.Features.Users;
  
  public record CreateUserCommand(string UserName, string Email) : IRequest<UserResponse>;
  
  public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
  {
      public CreateUserCommandValidator()
      {
          RuleFor(x => x.UserName).NotEmpty();
          RuleFor(x => x.Email).EmailAddress();
      }
  }
  
  public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserResponse>
  {
      private readonly IUnitOfWork _unitOfWork;
      
      public CreateUserCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
      
      public async Task<UserResponse> Handle(CreateUserCommand request, CancellationToken ct)
      {
          // Lógica: crear usuario, guardar, retornar DTO sin Id
      }
  }
  ```

### DTOs de Response

- **Sin `Id`:** respuestas que retorna el backend **nunca incluyen el Id** (según el workspace). Excepto en listados paginados, donde va en `items`.
  ```csharp
  public record UserResponse(string UserName, string Email);
  // NOT: public record UserResponse(Guid Id, string UserName, ...);
  ```
- **Solicitudes (request body):** pueden tener identificadores si es necesario pasarlos en el body.

### Excepciones de dominio

- **Ubicación:** `Domain/Exceptions/`.
- **Uso:** lanzan en handlers, validadores de dominio o servicios.
- **Ejemplo:**
  ```csharp
  throw new DomainException("El usuario ya tiene una membresía activa.");
  // → 409
  
  throw new NotFoundException("Usuario no encontrado.");
  // → 404
  ```

### Enums de estado

- **Ubicación:** `Domain/Enums/`.
- **Serialización:** string snake_case en JSON.
- **Ubicación en response:** deben estar en el Domain (nunca hardcodeados en el Controller).

### Paginación

- **Request:** parámetros `page` (≥1) y `page_size` (1–100) en la URL.
- **Response:** `PagedResult<T>` con `items`, `page`, `page_size`, `total_count`.
- **Validators:** cada Query de listado valida estos parámetros:
  ```csharp
  RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
  RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
  ```

### Controllers

- **Plantilla mínima:**
  ```csharp
  [ApiController]
  [Route("resource")]
  public class ResourceController(IMediator _mediator) : ControllerBase
  {
      [HttpGet("{id:guid}")]
      public async Task<ActionResult<ResourceResponse>> Get(Guid id, CancellationToken ct)
      {
          var result = await _mediator.Send(new GetResourceQuery(id), ct);
          return Ok(result);
      }
  }
  ```
- **Sin lógica:** solo delegan a MediatR. Los Handlers en Application hacen todo.

---

## Dependencias y versiones

| Paquete | Versión | Licencia | Uso |
|---|---|---|---|
| `MediatR` | 12.5.0 | Apache-2.0 | CommandQuery Separation, Behaviors |
| `FluentValidation` | 12.1.0 | Apache-2.0 | Validación declarativa |
| `Microsoft.AspNetCore.OpenApi` | 10.0.0 | MIT | OpenAPI / Swagger |
| `Microsoft.NET.Test.Sdk` | 17.14.1 | MIT | Tests |
| `xunit` | 2.9.3 | Apache-2.0 | Test runner |
| `xunit.runner.visualstudio` | 3.1.4 | Apache-2.0 | Test explorer |
| `Microsoft.AspNetCore.Mvc.Testing` | 10.0.0 | MIT | Integration tests |

Central Package Management (`Directory.Packages.props`) centraliza todas las versiones; no hay deduplificación de `packages.lock.json`.

---

## Qué viene después

- **T-002:** Persistencia base (DbContext, EF Core, Npgsql, IUnitOfWork, repositorios).
- **T-006:** Keycloak (JWT Bearer, autenticación real, políticas de roles).
- **Módulos de negocio:** Identidad, Membresías, Horarios, Entrenamiento, Asistencia, Dashboards (cada uno con Commands/Queries, Validators, Handlers, Controllers).

---

## Notas sobre el código anterior

El CLAUDE.md del repositorio (`spartan-backend/CLAUDE.md`) describe el monolito anterior de un solo proyecto. Está **desactualizado** desde T-001:

| Sección | Antes (monolito) | Ahora (T-001) | Cambio |
|---|---|---|---|
| Proyectos | `SpartanBackend.csproj` (uno) | 4 capas + tests | Reescrito |
| Namespace | `SpartanBackend` | `SpartanGym.*` | Renombrado |
| Autenticación | JWT propio + BCrypt | (vacío hasta T-006) | Eliminado |
| ORM | EF + Npgsql + AppDbContext | (vacío hasta T-002) | Replanificado |
| Documentación | Scalar | OpenAPI (`/openapi/v1.json`) | Migrado |
| Manejo de errores | No hay centralizado | `DomainExceptionHandler` | Nuevo |

El CLAUDE.md original quedaría desfasado y debe ser reescrito por el equipo o la herramienta de documentación según las convenciones nuevas. **No se reescribió en T-001** porque lo aclaraba la spec: `documenter` lo evalúa al documentar.
