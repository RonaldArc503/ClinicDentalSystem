# ADR-003: JWT Authentication with Refresh Tokens

**Status:** Accepted

**Context:** The system needs stateless authentication that works across multiple microservices. Session-based auth would require shared session storage and introduce coupling. A simple JWT without refresh tokens would force users to re-login every time the token expires.

**Decision:** Use JWT Bearer authentication with refresh tokens:
- **Access token:** Short-lived (1 hour), contains claims (sub, email, unique_name, role, permission).
- **Refresh token:** Long-lived (7 days), opaque, stored in the database, one-time use with rotation.
- **Password hashing:** BCrypt.Net-Next with work factor 11.
- Each microservice validates access tokens using the same JWT signing key without calling Identity.API (stateless).

**Consequences:**
- Positive: Stateless authentication between microservices.
- Positive: Refresh token rotation provides security — if a token is stolen, it can only be used once (the replacement fails).
- Positive: Same error message "Invalid credentials." / "Invalid refresh token." prevents user enumeration.
- Negative: Requires a database round-trip for refresh token validation.
- Negative: Revocation requires a database call (logout marks all user tokens as revoked).
