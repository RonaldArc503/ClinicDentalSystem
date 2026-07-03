# ADR-002: CQRS with MediatR

**Status:** Accepted

**Context:** The Application layer needs a clean separation between read and write operations. Directly coupling controllers to repositories or services would mix concerns and make the code harder to test, extend, or parallelize.

**Decision:** Use MediatR to implement the CQRS pattern:
- Each use case (command or query) is a separate request class.
- Each use case has a dedicated handler class.
- Validation is applied automatically via MediatR pipeline behavior.
- Controllers send requests via `ISender` and receive typed responses.

**Consequences:**
- Positive: Each handler is independently testable and has a single responsibility.
- Positive: Cross-cutting concerns (validation, logging, transaction management) are added as pipeline behaviors without modifying handlers.
- Positive: Vertical slices — every feature is a folder with its Command + Validator + Handler + Response.
- Negative: Additional boilerplate for simple CRUD, but the consistency across all use cases is worth it.
