# ADR-001: Clean Architecture + Domain-Driven Design

**Status:** Accepted

**Context:** The project requires a modular, testable, and maintainable architecture that can evolve into multiple microservices. A traditional monolithic or N-tier architecture would couple business logic with infrastructure concerns and make it difficult to split the system later.

**Decision:** Use Clean Architecture with Domain-Driven Design (DDD) tactical patterns:
- **Domain layer** contains entities, value objects, aggregates, and domain invariants with no external dependencies.
- **Application layer** contains CQRS commands/queries, interfaces, and FluentValidation validators.
- **Infrastructure layer** implements interfaces defined in Application (JWT, BCrypt, email, etc.).
- **Persistence layer** implements repositories and EF Core DbContext with Fluent API configurations.
- **API layer** exposes REST endpoints via ASP.NET Core controllers with global middleware.

**Consequences:**
- Positive: Domain logic is isolated and testable without infrastructure concerns.
- Positive: Each layer has a clear responsibility and dependency direction (inward).
- Positive: Future microservices can reuse SharedKernel without modification.
- Negative: More boilerplate compared to a simple CRUD approach, but the long-term maintainability gain justifies it.
