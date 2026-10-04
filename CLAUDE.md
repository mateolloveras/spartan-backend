# Spartan Backend — Contexto para Claude

## Leer primero
Antes de cualquier tarea, leer el análisis completo del proyecto:
`molon-labe-analisis.md` en el repo `spartan-frontend` (raíz del repo)

Contiene: arquitectura, roles, pantallas diseñadas, endpoints definidos, orden de desarrollo y setup completo.

## Proyecto
Sistema de gestión de gimnasio llamado **Molon Labe**. API REST consumida por el frontend Angular.

## Stack
- .NET 10 (ASP.NET Core Web API)
- Entity Framework Core + Npgsql (PostgreSQL)
- JWT Bearer Authentication
- BCrypt para hashear contraseñas
- PostgreSQL 17 via Docker

## Arquitectura
Monolito modular — un solo proyecto .NET organizado por módulos. No microservicios.

## Estructura
```
spartan-backend/
├── Program.cs                  ← configuración central: JWT, CORS, EF, DI
├── SpartanBackend.csproj       ← dependencias NuGet
├── appsettings.json            ← ConnectionString, JWT config, Google ClientId
├── docker-compose.yml          ← levanta PostgreSQL 17
├── Modules/
│   ├── Auth/                   ← login email/password + Google OAuth (stub)
│   ├── Users/                  ← CRUD de usuarios (solo admin)
│   ├── Memberships/            ← planes y suscripciones
│   ├── Workouts/               ← rutinas y ejercicios
│   └── Schedules/              ← horarios y reservas de clases
├── Infrastructure/
│   ├── AppDbContext.cs         ← DbContext con todos los DbSets
│   └── Entities/               ← User, Membership, Workout, Schedule
└── Common/
    └── Middleware/             ← manejo global de errores (pendiente)
```

## Roles
Tres roles definidos en el enum `UserRole`: `Admin`, `Employee`, `Client`.
El rol se incluye en el JWT como claim `role` en minúscula.
Los endpoints se protegen con `[Authorize(Roles = "admin")]` etc.

## Base de datos
PostgreSQL via Docker. Para levantar:
```bash
docker compose up -d
```
Connection string ya configurada en `appsettings.json` apuntando al contenedor.

## Primer paso
1. `docker compose up -d` (levantar BD)
2. Completar el JWT Key en `appsettings.json` (mínimo 32 caracteres)
3. `dotnet tool install --global dotnet-ef` (si no está instalado)
4. `dotnet ef migrations add InitialCreate`
5. `dotnet ef database update`
6. `dotnet run`
7. Scalar (API docs) en `http://localhost:5000/scalar`

## Convenciones
- Cada módulo tiene su Controller, Service y carpeta DTOs
- Los servicios se inyectan via constructor (primary constructor de C#)
- Namespace raíz: `SpartanBackend`
- CORS configurado para `http://localhost:4200` (Angular dev server)
- Swagger habilitado solo en Development

## Forma de trabajo
Ambos integrantes del equipo trabajan en ambos repos. Cada uno se encarga de una feature completa (frontend + backend) por vez. Al arrancar una tarea nueva, verificar si hay cambios en el repo del compañero con `git pull`.

## Frontend
El repo del frontend es `spartan-frontend/` (mismo nivel que este repo).
Los diseños HTML de referencia están en `spartan-design/` dentro del repo `spartan-frontend`.
