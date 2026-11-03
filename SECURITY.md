# Security

This document describes the security controls implemented in SmartSpazaAPI, the assumptions a
deployment must satisfy, and the residual risks that are deliberately left to the operator or to a
product decision.

## Reporting

Do not open a public issue for a suspected vulnerability. Report it privately to the repository
owner with steps to reproduce and the affected endpoint/commit.

## Authentication and authorization

- ASP.NET Core Identity enforces password complexity, account lockout and unique emails.
- Email confirmation is required by default (`Auth:RequireEmailConfirmation=true`).
- Access tokens are JWTs signed with HS256 and validated for issuer, audience, lifetime, signing key
  and a single allowed algorithm.
- Refresh tokens are 256-bit random values, stored only as SHA-256 hashes, rotated on use, revoked
  atomically and detected on reuse. Reuse revokes every active token for that user.
- Changing or resetting a password revokes all active refresh tokens.
- API endpoints use explicit role policies and resource ownership checks:
  - Drivers can only access their own tracking, deliveries and settings.
  - Suppliers can only create/read their own deliveries and manage their own/global catalog.
  - Suppliers and shop owners may record inventory checkouts; drivers cannot.
  - Shop owners can only manage their own products, deliveries and ratings.
  - Delivery item and profile reads require a real delivery relationship.
- `change-password` derives the account from the access token, never from the request body.
- Email confirmation uses a single-use Identity token. The GET confirmation link is idempotent once
  the account is confirmed and is rate-limited with the other auth endpoints.
- Registration is restricted to `Supplier`, `SpazaOwner` and `Driver`; privileged roles cannot be
  self-assigned beyond that list.
- The auth endpoints are rate limited per client IP (10 requests/minute) on top of a global limiter.

## Data protection

- The legacy `Password`, `PasswordResetToken` and `TokenExpiration` columns were removed from
  `ApplicationUser`; Identity stores only password hashes.
- Refresh tokens are never stored or logged in plaintext.
- Soft-deleted products/delivery items are filtered from normal queries.
- Response DTOs do not contain password hashes, reset tokens or SMTP credentials.
- Production exception responses are generic; stack traces are only returned in Development.
- `appsettings.Development.json` is excluded from the publish output, so the local JWT placeholder and
  connection string are not baked into the production image.

## Transport and headers

- Outside Development, `Https:RequireHttps` defaults to `true` and a fail-closed middleware rejects
  plain-HTTP requests with `426 Upgrade Required` after forwarded headers are evaluated. This prevents
  a misconfigured client or proxy from silently sending passwords, JWTs or refresh tokens in clear text.
  The internal `/health` probe is the only HTTP exception. The Development HTTP launch profile is
  loopback-only; do not bind it to a network interface.
- HSTS is one year with subdomains enabled (preload is left off until the domain is ready).
- Responses set `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`,
  `Permissions-Policy` and a restrictive `Content-Security-Policy`.
- Auth responses are marked `Cache-Control: no-store`.
- CORS is allow-list based. An empty production allow-list denies cross-origin browser access rather
  than falling back to `*`.
- `AllowedHosts` should be set to the public API hostname in production; the default `*` is only
  appropriate for local development.
- Forwarded headers are only trusted from loopback or explicitly configured proxy IPs
  (`ForwardedHeaders__KnownProxies__0`, `…__1`). The reverse proxy must overwrite `X-Forwarded-For`
  and `X-Forwarded-Proto`; never trust client-supplied values.
- SMTP requires TLS (`Smtp:EnableSsl=true`) by default. Plain SMTP is rejected unless
  `Smtp:AllowInsecure=true`, used only by the local Mailpit catcher. `SmtpClient` uses the system
  certificate trust store and the code does not disable certificate validation.
- SQL connections use `Encrypt=True`; the local Compose stack uses `TrustServerCertificate=True`
  only for the self-signed local SQL container. Production must use a trusted certificate.

## Abuse resistance

- Per-IP fixed-window limiters for auth (10/minute) and globally (200/minute).
- Login lockout after 5 failed attempts for 5 minutes.
- Account-existence responses are generic for login, forgot-password and resend-confirmation.
- Malformed confirmation/reset tokens return 400, not 500.
- Registration is resilient to mail outages: the account is created, the response reports
  `confirmationEmailSent=false`, and the user can request another email. Forgot-password and
  resend-confirmation always return the same generic response, even when SMTP fails, so account
  existence is not disclosed by mail errors.

## Secrets and configuration

- `Jwt:Key`, connection strings, SMTP credentials and email URLs are supplied by user secrets or
  environment variables. `Jwt:Key` must be at least 32 bytes and is required at startup.
- Environment variables are visible to the host/container runtime; production should source them from
  a secrets manager and restrict access to the process/container.
- The Docker Compose stack requires `JWT_KEY` and `MSSQL_SA_PASSWORD`; it contains no default
  database password.
- `.gitleaks.toml` allowlists the clearly-marked Development placeholder, the documented demo
  password and the **revoked** historical key so the CI secret scan is actionable.

## Dependency and CI security

- `dotnet list package --vulnerable --include-transitive` is clean for every project.
- CI builds with warnings-as-errors and tests on every push/PR, builds the Docker image, runs Gitleaks
  against full history and performs a transitive dependency audit. Dependabot monitors NuGet and
  GitHub Actions weekly.
- The container runs as the non-root `APP_UID` and has a health check.

## Known residual risks / deployment requirements

1. **Rotate the historical JWT key.** A signing key was committed before secrets were removed from
   `appsettings.json`. It is still in git history and must be treated as compromised. Rotate it in
   every environment; the Gitleaks allowlist does not make it safe.
2. **Privileged self-registration.** Anyone can register as Supplier, SpazaOwner or Driver. If those
   roles should be verified, add an approval/KYC step before activating the profile.
3. **Rate limiting is in-memory.** A multi-replica deployment needs a distributed limiter (Redis or
   an API gateway); otherwise each replica allows its own quota.
4. **Access-token lifetime after password change.** Refresh tokens are revoked immediately, but an
   already-issued access token remains valid until it expires (15 minutes in Production). Shorten
   the lifetime or add a token-version/security-stamp check if that window is unacceptable.
5. **Local Docker is not production.** Compose uses the `sa` SQL login (`TrustServerCertificate=True`),
   binds SQL and the API to localhost, and serves the API over plain HTTP on port 8080 with
   `Https:RequireHttps=false`. Production must use TLS termination, a least-privilege database user,
   a trusted certificate and a private network, with `Https:RequireHttps=true`. The Development
   connection string also trusts the local SQL certificate; do not point it at a remote database.
6. **TLS termination is external.** The API does not ship a production certificate. The reverse proxy
   must use a trusted certificate, disable obsolete TLS versions/ciphers, and overwrite forwarded
   headers. Certificate pinning is not implemented; standard PKI is used.
7. **Email-link tokens are in URLs.** Confirmation and password-reset tokens are single-use and
   time-limited, but the front-end pages must use HTTPS and a strict `Referrer-Policy`/no third-party
   resources to avoid token leakage through URLs, browser history or referrer headers. Reverse proxies
   should redact query strings on auth routes and never log Authorization/refresh-token values. The API
   enforces HTTPS for non-loopback email links, but cannot control the front-end page.
8. **HSTS preload is not enabled.** Enable preload and submit the domain once all subdomains are
   permanently HTTPS-only.
9. **Swagger UI is enabled in Development and the localhost-only Compose demo.** It is disabled in
   Production by default. If `Swagger:Enabled` is turned on in a shared environment, protect
   `/swagger` with authentication or a private network; it exposes the API surface and interactive
   calls.
10. **Email deliverability/security is external.** Configure SPF, DKIM, DMARC and TLS for the SMTP
    provider. `Smtp:EnableSsl` is true by default; plaintext SMTP is rejected unless
    `Smtp:AllowInsecure=true`, which is only used by the local Mailpit demo.
11. **No MFA or tamper-proof audit trail.** Security events are logged, but there is no central,
    append-only audit store or MFA. Add these if required by the assessment or regulator.
12. **No order/audit record for checkout.** Stock changes are atomic, but checkout does not yet write
    an order or stock-movement audit row.
13. **No LICENSE file.** If the project is intended to be released under the MIT licence, add a
    `LICENSE` file with the chosen copyright holder.
14. **Data-at-rest protection is external.** Enable TDE or encrypted volumes and protect backups;
    the application does not configure database encryption.

## Verification

```bash
dotnet build SmartSpazaAPI.slnx -c Release -warnaserror
dotnet test SmartSpazaAPI.Tests/SmartSpazaAPI.Tests.csproj -c Release
dotnet list SmartSpazaAPI.slnx package --vulnerable --include-transitive
gitleaks dir . --config .gitleaks.toml
gitleaks git . --config .gitleaks.toml
```

The test suite covers exception mapping, validators, claim helpers, pagination bounds, delivery
defaults, checkout stock rules, vehicle ownership, password-change identity, HTTPS enforcement,
security headers and SMTP TLS configuration. Live verification against SQL Server covered
registration, login, refresh rotation/reuse, revocation, password changes, role/ownership denials,
checkout stock rules, tracking, rate limiting, HTTPS/426/HSTS behavior, CORS, JWT algorithm
restriction and the full Mailpit confirmation/reset email flow.

## Assessment checklist

- [x] Authentication, role authorization and resource ownership
- [x] Password policy, lockout, email confirmation and password reset
- [x] JWT validation and refresh-token rotation/reuse detection
- [x] Input validation (DataAnnotations + FluentValidation) and safe ORM queries
- [x] Generic production errors and security headers
- [x] CORS allow-list, rate limiting and trusted forwarded headers
- [x] HTTPS enforcement (426), HSTS, SMTP TLS and SQL encryption settings
- [x] Dependency vulnerability audit and secret scan in CI
- [x] Unit tests and live integration verification of the main security flows
- [ ] Rotate the historical signing key and purge it from history
- [ ] Production TLS certificate/reverse proxy and trusted forwarded-header configuration
- [ ] Front-end HTTPS, no-referrer and reset-page token handling
- [ ] Approval/KYC for privileged roles (if required)
- [ ] Distributed rate limiting if running more than one API replica
- [ ] Central audit logging/MFA (if required)
