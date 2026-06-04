# Contributing to TapHoa Backend

## Prerequisites

- .NET 10 SDK
- Docker + Docker Compose (PostgreSQL + Redis)
- An OTLP-compatible collector (optional, e.g. Jaeger) for OpenTelemetry

## Local setup

```bash
# 1. Clone and restore
git clone <repo>
cd TapHoa2
dotnet restore

# 2. Start dependencies
docker compose up -d postgres redis

# 3. Copy and fill local config
cp src/Presentation/TapHoa.Api/config/appsettings.json.example \
   src/Presentation/TapHoa.Api/config/appsettings.json
# → edit Jwt:Key, ConnectionStrings, etc.

# 4. Apply migrations
dotnet ef database update \
  --project src/Infrastructure/TapHoa.Persistence \
  --startup-project src/Presentation/TapHoa.Api

# 5. Run
dotnet run --project src/Presentation/TapHoa.Api
```

## Branch & commit conventions

| Branch prefix | Purpose |
|---------------|---------|
| `feat/`       | New feature |
| `fix/`        | Bug fix |
| `chore/`      | Tooling / dependencies |
| `docs/`       | Documentation only |

Commits follow **Conventional Commits**:
```
feat(orders): add wallet partial payment support
fix(auth): correct refresh token expiry calculation
```

## Pull Request checklist

- [ ] All existing tests pass (`dotnet test`)
- [ ] New public logic has unit tests in the relevant `*.Tests` project
- [ ] No compiler warnings introduced
- [ ] Migration added if schema changed (`dotnet ef migrations add <Name>`)
- [ ] CHANGELOG.md updated under `[Unreleased]`

## Code style

- Follow `.editorconfig` (4-space indent, LF, no trailing whitespace)
- `GlobalUsings.cs` for commonly used namespaces — don't repeat them per-file
- Use `Result<T>` pattern for domain errors; throw only for truly exceptional cases
- Prefer primary constructors for services

## Running tests

```bash
dotnet test                        # all projects
dotnet test --filter "FullyQualifiedName~Orders"   # specific area
dotnet test --collect:"XPlat Code Coverage"        # with coverage
```
