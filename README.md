# DentalClinicSystem

![Build](https://github.com/RonaldArc503/ClinicDentalSystem/actions/workflows/ci.yml/badge.svg)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

Enterprise-grade dental clinic management system built with **Clean Architecture**, **CQRS**, and **microservices** on **ASP.NET Core 8**.

---

## Architecture

```
                 Client
                    │
                    ▼
            API Gateway (YARP) — future
           ┌───────────┴───────────┐
           ▼                       ▼
    Identity.API             Patients.API
    (auth/users)             (patients CRUD)
           │                       │
           ▼                       ▼
      SQL Server              SQL Server
```

Each microservice follows a **vertical slice** approach within Clean Architecture layers:

```
Service/
├── Domain         — Entities, ValueObjects, invariants
├── Application    — CQRS commands/queries, interfaces, validators
├── Infrastructure — External services (JWT, BCrypt, email)
├── Persistence    — EF Core DbContext, repositories, migrations/seed
└── API            — Controllers, middleware, Program.cs
```

Shared building blocks (`SharedKernel`) are reused across all services.

---

## Technologies

| Category | Technology |
|---|---|
| Runtime | .NET 8 (LTS) |
| Architecture | Clean Architecture + DDD |
| CQRS | MediatR |
| Validation | FluentValidation |
| ORM | Entity Framework Core 8 |
| Database | SQL Server |
| Auth | JWT (access + refresh tokens) |
| Password Hashing | BCrypt.Net-Next |
| Logging | Serilog (console + rolling file) |
| API Docs | Swagger / Swashbuckle |
| Error Handling | ProblemDetails (RFC 7807) |
| Health Checks | ASP.NET Core Health Checks |
| Containerization | Docker (planned) |
| CI/CD | GitHub Actions |

---

## Solution Structure

```
DentalClinicSystem/
│
├── src/
│   ├── BuildingBlocks/
│   │   └── SharedKernel/          — Base classes (Entity, ValueObject, Result, Middleware)
│   │
│   └── Services/
│       ├── Identity/              — Auth microservice (users, roles, permissions, JWT)
│       │   ├── Identity.Domain/
│       │   ├── Identity.Application/
│       │   ├── Identity.Infrastructure/
│       │   ├── Identity.Persistence/
│       │   └── Identity.API/
│       │
│       └── Patients/              — Coming soon
│           ├── Patients.Domain/
│           ├── Patients.Application/
│           ├── Patients.Persistence/
│           └── Patients.API/
│
├── tests/
│   └── Identity.Application.Tests/  — 10 unit tests (Login + RefreshToken)
│
├── docs/
│   ├── architecture/
│   ├── api/
│   ├── database/
│   ├── deployment/
│   └── decisions/                   — Architecture Decision Records
│
├── scripts/
├── docker/
├── .github/workflows/ci.yml
│
├── Directory.Build.props
├── Directory.Packages.props
├── .editorconfig
├── .gitignore
├── AGENTS.md
└── DentalClinicSystem.slnx
```

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/sql-server) (local or Docker)
- [Git](https://git-scm.com/)

### Clone

```bash
git clone https://github.com/RonaldArc503/ClinicDentalSystem.git
cd DentalClinicSystem
```

### Configure Connection String

Edit `src/Services/Identity/Identity.API/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=ClinicDental;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

### Run

```bash
dotnet build

# Run Identity.API (use DLL due to Windows App Control)
dotnet src/Services/Identity/Identity.API/bin/Debug/net8.0/Identity.API.dll
```

The API will be available at `http://localhost:5048`.

Swagger UI at `http://localhost:5048/swagger`.

---

## Testing

```bash
dotnet test
```

All tests should pass with **0 warnings, 0 errors**.

---

## API Documentation (Identity)

| Method | Path | Auth | Description |
|---|---|---|---|
| POST | `/api/v1/auth/register` | No | Register new user |
| POST | `/api/v1/auth/login` | No | Login, returns JWT + refresh token |
| POST | `/api/v1/auth/refresh` | No | Exchange refresh token for new JWT |
| POST | `/api/v1/auth/logout` | Yes | Revoke all user refresh tokens |
| GET | `/api/v1/auth/me` | Yes | Current user info + roles + permissions |
| GET | `/health` | No | Health check (API + SQL Server) |

---

## Roadmap

- [x] **Identity.API** — Authentication, JWT, roles & permissions
- [ ] **Sprint 0.2** — CI/CD, README, ADRs, quality
- [ ] **Patients.API** — CRUD, clinical history, odontogram
- [ ] **Docker Compose** — SQL Server + Identity + Patients
- [ ] **API Gateway** — YARP reverse proxy
- [ ] **Integration Events** — RabbitMQ / MassTransit
- [ ] **Appointments.API**
- [ ] **Billing.API**

---

## License

[MIT](LICENSE) © 2026 Ronald Arc
