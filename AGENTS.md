# AGENTS.md — DentalClinicSystem

## Build & Test
```powershell
# Build entire solution
dotnet build

# Run all tests
dotnet test

# Run tests without rebuild
dotnet test --no-build

# Build a specific project
dotnet build src/Services/Identity/Identity.API
```

## Architecture
- **Clean Architecture** with vertical slice per feature (Command + Validator + Handler + Response in one folder)
- **SharedKernel**: Entity\<TId\>, BaseEntity\<TId\>, AuditableEntity\<TId\>, ValueObject, Result\<T\>, Error

## Solution Structure
```
src/
  BuildingBlocks/SharedKernel/
  Services/Identity/
    Identity.Domain/       — Entities, ValueObjects (Email, Username, PersonName, Password)
    Identity.Application/  — CQRS commands, interfaces, validators
    Identity.Persistence/  — EF Core DbContext, Fluent API configs, repositories, migrations, seed
    Identity.Infrastructure/ — JWT, BCrypt, external services
    Identity.API/          — Controllers, Program.cs
tests/
  Identity.Application.Tests/
docs/
scripts/
docker/
```

## Conventions
- **Manual mapping** — No AutoMapper
- **HasConversion** for single-value Value Objects (Email, Username); `OwnsOne` for multi-value (PersonName)
- **Repository pattern** with typed methods (Email email, Username username) not raw strings
- Separate `IRefreshTokenRepository` from `IUserRepository`
- All handlers throw `InvalidOperationException` for business rule violations (single error message for security: "Invalid credentials.", "Invalid refresh token.")
- **No comments in code** — clean, self-documenting code
- `sealed` on all handlers, validators, repositories, services
- `private set` for domain entity properties
- `private` constructors + static factory `Create` methods for entities

## Exception Handling
- **Global Exception Middleware** (`SharedKernel.Middleware.ExceptionMiddleware`)
- Returns **ProblemDetails (RFC 7807)** JSON for all errors:
  | HTTP | Exception | Description |
  |---|---|---|
  | 400 | `ValidationException` | FluentValidation failures |
  | 400 | `InvalidOperationException` | Business rule violations |
  | 400 | `DomainException` | Domain rule violations |
  | 404 | `NotFoundException` | Entity not found |
  | 409 | `ConflictException` | Duplicates / conflicts |
  | 500 | `Exception` (unhandled) | Internal server error |
- **ValidationBehavior** (MediatR pipeline) auto-runs FluentValidation validators before handlers

## Logging
- **Serilog** with structured JSON output
- Console + rolling file (`logs/identity-{date}.txt`, 30-day retention)
- Enrichment: MachineName, EnvironmentName

## Health Checks
| Endpoint | Checks |
|---|---|
| `GET /health` | API liveness, SQL Server connectivity |

## JWT Claims
- `sub`: user ID
- `email`: email
- `unique_name`: username
- `role`: role names (multiple)
- `permission`: permission names (multiple, from role-permission mapping)

## Authorization Policies (Program.cs)
Policies use constants from `Identity.Application.Constants.Permissions`:
| Policy | Requirement |
|---|---|
| RequireAdminRole | Role = Admin |
| RequireDentistRole | Role = Dentist |
| `Permissions.Patients.Read` | permission = patients.read |
| `Permissions.Patients.Write` | permission = patients.write |
| `Permissions.Appointments.Read` | permission = appointments.read |
| `Permissions.Appointments.Write` | permission = appointments.write |
| `Permissions.Billing.Read` | permission = billing.read |
| `Permissions.Billing.Write` | permission = billing.write |
| `Permissions.Treatments.Read` | permission = treatments.read |
| `Permissions.Treatments.Write` | permission = treatments.write |

## Seed Data
- Roles: Admin, Dentist, Patient, Receptionist
- 15 permissions across 5 categories (Users, Patients, Appointments, Billing, Treatments)
- Role-permission mappings applied on first run
- Admin user created if AdminPassword config is set

## NuGet Packages (Identity microservice)
| Package | Version |
|---|---|
| EF Core (Core, SqlServer, Design) | 8.0.11 |
| MediatR | 14.2 |
| FluentValidation | 12.1.1 |
| BCrypt.Net-Next | 4.2.0 |
| Microsoft.AspNetCore.Authentication.JwtBearer | 8.0.11 |
| System.IdentityModel.Tokens.Jwt | 8.0.2 |
| Serilog.AspNetCore | 8.0.3 |
| Serilog.Sinks.File | 6.0.0 |
| Serilog.Enrichers.Environment | 3.0.1 |
| AspNetCore.HealthChecks.SqlServer | 8.0.2 |
| Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore | 8.0.11 |

## API Endpoints (Identity.API)
| Method | Path | Auth | Description |
|---|---|---|---|
| POST | /api/v1/auth/register | No | Register new user |
| POST | /api/v1/auth/login | No | Login, returns JWT + refresh token |
| POST | /api/v1/auth/refresh | No | Exchange refresh token for new JWT |
| POST | /api/v1/auth/logout | Yes | Revoke all user refresh tokens |
| GET | /api/v1/auth/me | Yes | Current user info + roles + permissions |
| GET | /health | No | Health check (API + SQL Server) |

## Pipeline (middleware order)
```
ExceptionMiddleware
  → SerilogRequestLogging
    → Swagger (dev)
      → Seed Data
        → Authentication
          → Authorization
            → Controllers / HealthChecks
```

## Next Steps
1. Patients microservice (reuse SharedKernel + Identity patterns)
2. Role/Permission management endpoints (admin CRUD)
3. Email confirmation flow
4. Password reset flow
