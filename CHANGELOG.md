# Changelog

All notable changes to **TapHoa Backend** will be documented in this file.

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- OpenTelemetry traces + metrics (OTLP export)
- Health check endpoint `/health` with DB + Redis checks
- Audit logging middleware for mutating requests
- `GlobalUsings.cs` for each project
- `Directory.Build.props` with Roslyn Analyzers
- `.editorconfig` for consistent code style
- MassTransit + RabbitMQ consumer for `OrderPaidMessage`

### Changed
- Migrated logging from NLog → Serilog (structured logging, daily rolling files)
- Auto-migrate now only runs in Development; Production uses manual `dotnet ef database update`
- Scalar/Swagger enabled in Staging environment (requires JWT auth)

### Security
- Removed hardcoded JWT key from `config/appsettings.json`
- Removed hardcoded email fallback from User-Agent header

## [1.0.0] - 2026-01-01

### Added
- Initial release
- Auth: JWT, refresh token, rate limiting, email confirmation, password reset
- Products, Categories, Cart, Orders, Flash Sale, Reviews, Articles
- Wallet: top-up via SePay webhook, wallet payment
- Admin, Agent, Driver portals
- Redis caching for products and flash sale
- NightBatchJob for overnight order processing
- Docker Compose with PostgreSQL + Redis
