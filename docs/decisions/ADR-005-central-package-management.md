# ADR-005: Central Package Management

**Status:** Accepted

**Context:** With multiple projects (SharedKernel + 4 Identity layers + tests), NuGet package versions were duplicated across `.csproj` files. Updating a package required editing multiple files, and version mismatches could cause runtime conflicts.

**Decision:** Use MSBuild Central Package Management:
- `Directory.Packages.props` at the solution root lists all package versions.
- Each `.csproj` references packages without a `Version` attribute.
- `ManagePackageVersionsCentrally` is set to `true`.
- Common build properties (`TargetFramework`, `Nullable`, `ImplicitUsings`, `TreatWarningsAsErrors`) are centralized in `Directory.Build.props`.

**Consequences:**
- Positive: Single source of truth for all package versions — one file to update.
- Positive: Consistent versions across projects, eliminating version conflicts.
- Positive: New microservices automatically inherit the same package versions and build properties.
- Negative: Package-specific attributes (PrivateAssets, IncludeAssets) still need to stay in individual `.csproj` files.
