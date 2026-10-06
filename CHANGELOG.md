# Changelog — SpartanGym Backend

## [2026-10-05] — T-001 Solución .NET base: capas, pipeline y convenciones HTTP

### Agregado
- Solución .NET 10 con cuatro capas de Clean Architecture: Domain, Application, Infrastructure, Presentation.
- Excepciones de dominio: `NotFoundException`, `ValidationException`, `DomainException`, `UnauthorizedException`.
- Pipeline de validación integrado con MediatR 12.5.0; un error de validación responde 400 con detalle de campos en snake_case.
- Manejo global de excepciones de dominio: 404, 400, 401, 409 y 500, con mensajes en español.
- Convenciones HTTP transversales: JSON en snake_case, enums como string snake_case, respuestas en formato `ProblemDetails`.
- `PagedResult<T>` disponible para listados paginados.
- Endpoint público `GET /api/health` para prueba del pipeline HTTP.
- OpenAPI disponible en desarrollo en `/api/openapi/v1.json`.
- CORS configurado para `http://localhost:4200` (frontend local).
- Tests unitarios e integración para validación y manejo de errores (27 tests verdes).

### Cambiado
- `.gitignore`: agregadas entradas para `TestResults/` y `*.coverage`.

### Eliminado
- Código monolítico anterior: `SpartanBackend.csproj`, Controllers del tipo Service+DTO, módulos hardcodeados, DbContext y Entities (reemplazados por T-002).
- Dependencias JWT propio, BCrypt, Google OAuth (migran a T-006 Keycloak).
- Paquetes Scalar (documentación de API).
