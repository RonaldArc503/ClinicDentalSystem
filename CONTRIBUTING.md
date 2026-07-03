# Contributing

## Commit Convention

Use [Conventional Commits](https://www.conventionalcommits.org/):

- `feat:` new feature
- `fix:` bug fix
- `refactor:` code change that neither fixes nor adds
- `chore:` tooling, CI, dependencies
- `docs:` documentation
- `test:` adding or fixing tests
- `style:` formatting, linting

## Branch Flow

- `main` — production-ready
- `feature/<name>` — new features, merge via PR
- `fix/<name>` — bug fixes, merge via PR

## Before Committing

1. `dotnet build` — must pass with 0 errors, 0 warnings
2. `dotnet test` — all tests must pass

## Code Style

- Follow `.editorconfig` rules
- No comments in code (self-documenting)
- Manual mapping only (no AutoMapper)
- `sealed` on handlers, validators, repositories, services
- `private set` for domain properties
- `private` constructors + `static Create` factory methods for entities
