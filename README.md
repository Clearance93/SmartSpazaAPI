# SmartSpazaAPI

Backend REST API for **SmartSpaza** — a last-mile delivery and inventory platform for spaza shops
(South African informal convenience stores). Built with .NET 10, EF Core (SQL Server), ASP.NET Core
Identity and JWT authentication.

## Architecture

The solution is a layered .NET solution (`SmartSpazaAPI.slnx`):

| Project | Responsibility |
| --- | --- |
| `SmartEnums` | Enumerations and shared role names (`DeliveryStatus`, `ShopType`, `RoleNames`) |
| `SmartModels` | EF Core entities (`ApplicationUser`, `Delivery*`, `Product`, `RefreshToken`, …) |
| `SmartDto` | Request/response DTOs |
| `SmartInterfaces` | Repository + service contracts |
| `SmartData` | `ApplicationDbContext` and EF Core migrations |
| `SmartRepository` | EF Core repositories (generic + specialised) |
| `SmartCore` | Unit of Work and AutoMapper profile |
| `SmartServices` | Business services (auth, delivery, settings, products) and FluentValidation validators |
| `SmartUtility` | Dependency injection, role/product seeder, Dapper type handlers |
| `SmartConfiguration` | Exception types and global exception-handling middleware |
| `SmartSpazaAPI` | Web host: controllers, configuration, filters, health checks |
| `SmartSpazaAPI.Tests` | xUnit unit tests |

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (local instance, Docker, or Azure SQL)
- (Optional) Docker Desktop for the containerised workflow

## Configuration & secrets

**Secrets are never committed.** `Jwt:Key`, the connection string and SMTP credentials are supplied
at runtime.

Local development uses [user secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets):

```bash
cd SmartSpazaAPI
dotnet user-secrets set "Jwt:Key" "<a-random-string-of-at-least-32-bytes>"
```

`appsettings.Development.json` provides a local connection string and a placeholder JWT key so the
app runs out of the box; override it with user secrets or environment variables for anything real.

Production configuration is supplied via environment variables (double underscore for nesting):

| Variable | Purpose |
| --- | --- |
| `ConnectionStrings__ConnectionString` | SQL Server connection string (`Encrypt=True`; trusted certificate in production) |
| `Jwt__Key` | Signing key (≥ 32 bytes, required outside Development) |
| `Jwt__Issuer` / `Jwt__Audience` | Token issuer/audience |
| `AllowedHosts` | API hostname(s) allowed by host filtering; set the real domain instead of `*` in production |
| `Cors__AllowedOrigins__0`, `…__1` | Allowed front-end origins (empty denies cross-origin in Production) |
| `Auth__RequireEmailConfirmation` | `true` by default; when enabled, valid SMTP settings are required at startup |
| `Https__RequireHttps` | `true` outside Development; rejects plain-HTTP requests with 426 |
| `Swagger__Enabled` | `true` in Development, `false` in Production by default |
| `ForwardedHeaders__KnownProxies__0` | Optional trusted reverse-proxy IPs for client-IP rate limiting |
| `Smtp__Host`, `Smtp__Port`, `Smtp__EnableSsl`, `Smtp__AllowInsecure`, `Smtp__Username`, `Smtp__Password`, `Smtp__FromAddress` | Outbound email (`AllowInsecure` is for local mail catchers only) |
| `AppSettings__EmailConfirmationUrl`, `AppSettings__PasswordResetUrl` | Front-end URLs used in emails |

The API fails fast at startup when:

- `Jwt:Key` is missing or shorter than 32 bytes outside Development.
- Email confirmation is enabled but SMTP or the email links are not configured.
- EF Core migrations fail (the process does not start against a stale schema).

## Running locally

```bash
dotnet restore SmartSpazaAPI.slnx
dotnet run --project SmartSpazaAPI --launch-profile https
```

Use the HTTPS launch profile; do not expose the HTTP profile to a network. The HTTP profile is only
for loopback development.

### Debugging from the IDE

Open `SmartSpazaAPI.slnx`, set `SmartSpazaAPI` as the startup project and press **F5**. Both launch
profiles set `ASPNETCORE_ENVIRONMENT=Development` and open `/swagger` automatically; the API root `/`
also redirects to Swagger when it is enabled. Breakpoints in controllers and services work normally.
If port 5223 is already in use, stop the terminal API first.

- API base URL (HTTPS profile): `https://localhost:7084`
- Health check: `GET /health`
- Swagger UI (Development by default): `GET /swagger`
- OpenAPI JSON: `GET /swagger/v1/swagger.json`

EF Core migrations are applied automatically at startup. In Development the seeder also creates
demo users, profiles, products and driver settings; other environments only seed roles.

> Upgrading an existing database: the `ProductBarcodeUniqueIndex` migration requires existing,
> non-deleted products to have unique barcodes. Remove duplicates before upgrading or startup will
> fail.

## Tests

```bash
dotnet test SmartSpazaAPI.Tests/SmartSpazaAPI.Tests.csproj -c Release
```

The suite covers exception mapping, validators, claim helpers, checkout stock rules, vehicle
ownership, delivery defaults and authenticated password changes.

## Docker

```bash
export JWT_KEY="$(openssl rand -base64 48)"
export MSSQL_SA_PASSWORD="$(openssl rand -base64 24)"
docker compose up --build
```

The API is then available at `http://localhost:8080` (health at `/health`). The compose stack runs
SQL Server 2022 and [Mailpit](https://mailpit.axllent.org/), a local SMTP catcher. It requires
`JWT_KEY` and `MSSQL_SA_PASSWORD` and binds the API, SQL and the Mailpit UI to localhost only.

This stack deliberately runs plain HTTP because it is a local-only demo (`Https__RequireHttps=false`).
Never publish port 8080 beyond localhost. For a real deployment, terminate TLS at a reverse proxy,
configure the proxy IP in `ForwardedHeaders__KnownProxies__0`, and leave
`Https__RequireHttps=true` so plain-HTTP requests are rejected.

### Production TLS

The API is designed to sit behind a TLS-terminating reverse proxy. The proxy must:

- present a certificate from a trusted CA for the public API hostname;
- allow only TLS 1.2/1.3;
- overwrite `X-Forwarded-For` and `X-Forwarded-Proto` (never pass client-supplied values);
- keep the API port on a private network or localhost.

Then set `Https__RequireHttps=true`, `AllowedHosts=<api-hostname>` and
`ForwardedHeaders__KnownProxies__0=<proxy-ip>`. Do **not** set `Smtp__AllowInsecure` in production and
do not use `TrustServerCertificate=True` outside the local SQL container. A Caddy example is provided
in [`deploy/Caddyfile`](deploy/Caddyfile).

Email confirmation is enabled in the compose stack. Register an account, open the Mailpit UI at
`http://localhost:8025`, and click the confirmation link — it points at the API's
`GET /api/Auth/confirm-email` endpoint, so it confirms the account directly. Then log in. Swagger is
available at `http://localhost:8080/swagger` in this localhost-only demo. Do not expose the Mailpit
UI, Swagger or the SQL port outside the host. The stack does not seed demo accounts; those are
Development-only. Password-reset links point at a front-end URL; this repository only contains the
API, so use the reset token from Mailpit when testing reset flows without a front end.

## Authentication

- Access tokens: JWT (HS256), short-lived (`Jwt:AccessTokenMinutes`, default 15).
- Refresh tokens: opaque, stored **hashed** (SHA-256), rotated on use, revoked atomically and
  revocable (`POST /api/Auth/refresh-token`, `POST /api/Auth/revoke-token`). Reusing a revoked
  token revokes all of the user's active sessions.
- Changing or resetting a password revokes all active refresh tokens.
- Registration requires the role to be one of `Supplier`, `SpazaOwner`, `Driver`, and the account
  must confirm its email before it can log in (unless `Auth:RequireEmailConfirmation=false`).
- Login lockout and password policy are enforced by ASP.NET Core Identity.
- Password reset tokens are emailed and never returned in an API response.
- Auth endpoints are rate limited per client IP (fixed window, 10 requests/minute) on top of a
  partitioned global limiter.

### Key endpoints

| Method | Route | Auth |
| --- | --- | --- |
| POST | `/api/Auth/registration` | anonymous |
| POST | `/api/Auth/login` | anonymous |
| POST | `/api/Auth/refresh-token` | anonymous |
| POST | `/api/Auth/revoke-token` | anonymous |
| POST | `/api/Auth/forgot-password` | anonymous |
| POST | `/api/Auth/reset-password` | anonymous |
| POST | `/api/Auth/change-password` | authenticated (uses the token identity) |
| GET/PUT | `/api/Settings/*` | Driver (own profile only) |
| POST | `/api/Delivery/createDelivery` | Supplier (own deliveries) |
| GET | `/api/Delivery/allDeliveries` | Supplier (own deliveries) |
| POST/PUT/GET | `/api/Delivery/*tracking*`, `/api/Delivery/driver*` | Driver (own records) |
| GET | `/api/Delivery/deliveryById` | Delivery participants only |
| POST | `/api/Delivery/upsert` | SpazaOwner (rating own deliveries) |
| POST/GET | `/api/Delivery/*delivery-item*` | Delivery participants (Supplier for writes) |
| GET | `/api/Products/*` | authenticated |
| POST/PUT/DELETE | `/api/Products/*` | Supplier or SpazaOwner (own products) |
| POST | `/api/Products/checkout` | Supplier or SpazaOwner |

## Demo accounts

The **Development** seeder creates three confirmed accounts (password `Spaza_123`) together with
their supplier, shop, driver, availability, navigation and notification rows:

| Role | Email |
| --- | --- |
| Spaza owner (retailer) | `demo@smartspaza.co.za` |
| Supplier | `supplier@smartspaza.co.za` |
| Driver | `driver@smartspaza.co.za` |

Demo data is only seeded in Development.

## CI

`.github/workflows/ci.yml` restores, audits, builds (warnings as errors) and tests the solution on
every push/PR, builds the Docker image, and runs a Gitleaks secret scan using `.gitleaks.toml`.
Dependabot monitors NuGet and GitHub Actions dependencies weekly.

## Security note

A signing key was committed to `appsettings.json` in the repository's history before secrets were
moved out of source control. It must be treated as compromised: rotate it in every environment that
may have used it. The Gitleaks allowlist only prevents the historical value from failing secret
scanning; it does not make the key safe.

Swagger is enabled in Development only by default. If you enable `Swagger:Enabled` in a shared
environment, put it behind authentication or a private network.

See [`SECURITY.md`](SECURITY.md) for the full security model, residual risks and the assessment
checklist.

## Hosting the Angular UI on this site

The site also serves the Smart Spaza Angular app from `wwwroot`, so one deployment exposes
both the UI and the API:

| Path | Content |
| --- | --- |
| `/` and client routes (`/login`, `/scanner`, `/dashboard`, …) | Angular UI (`wwwroot/index.html`, SPA fallback) |
| `/api/*` | API controllers |
| `/swagger` | Swagger UI |
| `/health` | database health check |

`Program.cs` wires `UseDefaultFiles()` + `UseStaticFiles()` and a fallback that returns
`index.html` for client routes, while unknown `/api` paths and missing files still 404.
`SecurityHeadersMiddleware` applies a CSP that allows the Angular bundle and camera access
for the scanner; API responses keep the strict `default-src 'none'` policy.

Build/copy the UI from the `Smart-Spaza-UI` repo:

```bash
npm run deploy:api        # ng build + copy dist/Smart-Spaza-UI/browser -> SmartSpazaAPI/wwwroot
dotnet publish SmartSpazaAPI/SmartSpazaAPI.csproj -c Release -o publish
```

Production app settings (host environment, not committed): `ConnectionStrings__ConnectionString`,
`Jwt__Key`, `Jwt__Issuer`, `Jwt__Audience`, `Https__RequireHttps`, `Auth__RequireEmailConfirmation`
and optional `Smtp__*` / `Swagger__Enabled`.
