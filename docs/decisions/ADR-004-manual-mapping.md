# ADR-004: Manual Mapping (No AutoMapper)

**Status:** Accepted

**Context:** The project needs to map between domain entities, DTOs, and command/response objects. AutoMapper is a popular choice but introduces implicit mapping behavior that can hide bugs, makes refactoring harder, and adds complexity with custom resolvers and value converters.

**Decision:** Use manual mapping in all cases:
- Command handlers directly map from command properties to domain objects.
- Responses are constructed explicitly using constructors or factory methods.
- No AutoMapper NuGet package is added to any project.

**Consequences:**
- Positive: Mapping logic is explicit and visible at a glance.
- Positive: Compiler errors catch missing or changed properties during refactoring.
- Positive: No runtime configuration issues or mapping profile maintenance.
- Negative: More verbose, especially for complex mappings. Handled by keeping mappings simple and colocated with the use case.
