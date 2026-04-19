# Phase 7 — Production Readiness (Hardening Pass)

This document summarizes the final hardening pass, deployment watchlist, release checklist, and permission audit.

---

## 1) Production readiness summary

### What was reviewed

- **Backend:** `Program.cs`, JWT / seed / CORS configuration, controller authorization model, identity seeding, health endpoint, auth & intelligence services for observability.
- **Frontend:** Session validation, login return URL handling, API client behavior (unchanged but validated against stricter session rules).

### What was fixed or added

| Area | Change |
|------|--------|
| **API authorization** | Global **authenticated-user** filter on all MVC controllers. `AuthController` remains `[AllowAnonymous]`. `GET /api/system/health` is `[AllowAnonymous]` so load balancers can probe without JWT. |
| **Production JWT** | **Fail-fast** at startup if `Jwt:SigningKey` still contains `LOCAL_DEV` or `REPLACE_IN_PRODUCTION` when `ASPNETCORE_ENVIRONMENT=Production`. |
| **Production seed defaults** | New `appsettings.Production.json`: `IdentitySeed:BootstrapAdmin` defaults to **false** (permissions/templates still seed when `RunOnStartup` is true). |
| **Bootstrap admin safety** | `IdentityDatabaseSeeder` throws if `BootstrapAdmin` is true and `AdminPassword` is empty (prevents blank-password admin). |
| **Startup warning** | If `BootstrapAdmin` is enabled in Production, a **single warning** is logged recommending one-time bootstrap then disable + rotate password. |
| **CORS** | Optional `Cors:AllowedOrigins` array: when non-empty, default CORS policy is registered and `UseCors()` runs after HTTPS redirection (Windows-friendly split hosting with Next.js). |
| **Logging** | `IntelligenceService`: structured **Information** logs on successful generation; **Error** logs on failed employee/cycle runs (no PII). `AuthService`: **Warning** on failed login (no email logged). |
| **Health** | Existing `GET /api/system/health` kept; explicitly anonymous for monitors. |
| **Frontend session** | `isSessionValid` now requires **both** `accessToken` and `expiresAtUtc` (avoids treating incomplete persisted state as valid). |
| **Login redirect** | `returnUrl` sanitized: only same-origin relative paths; blocks `/login` loops and `//evil` patterns. |

### Known non-blockers (acceptable follow-ups)

- **Fine-grained authorization:** Many business endpoints rely on **authentication + FluentValidation + domain rules**; not every controller action has a **permission policy** yet. HR workflows should assume least-privilege roles are assigned correctly.
- **CORS:** Empty `AllowedOrigins` means browser calls must be same-origin or use a reverse proxy; populate when SPA is on another origin.
- **Accessibility:** Focused fixes were not applied module-wide; continue incremental `aria-*` on high-traffic forms.

---

## 2) Backend permission audit (concise)

| Controller / area | Authorization model |
|-------------------|------------------------|
| **Default** | All controllers require **authenticated JWT** unless overridden. |
| `AuthController` | `[AllowAnonymous]` — login only. |
| `SystemController` | `health` — `[AllowAnonymous]`; any future system routes should be reviewed individually. |
| `UsersController`, `RolesController`, `PermissionsController` | Explicit policies (`USER_MANAGE`, `ROLE_MANAGE`). |
| `ApprovalRequestsController`, `NotificationsController`, `NotificationTemplatesController`, `IntelligenceController` | Class-level `[Authorize]` + per-action permission policies. |
| **Other domain controllers** (employees, performance, talent, succession, development, marketplace, analytics) | **Authenticated user required**; authorization is primarily **role/permission claims inside JWT** for UI and future policy expansion — ensure tokens are issued only to trusted users and roles are curated. |

**Recommendation:** For highest sensitivity (e.g. `DELETE`, admin operations), add explicit `[Authorize(Policy = …)]` per module in a future incremental pass.

---

## 3) Risk / watchlist (Windows Server)

1. **`NEXT_PUBLIC_API_BASE_URL`** — Must point to the published API (HTTPS). Wrong URL = silent UI failures.
2. **HTTPS / TLS** — Terminate TLS at IIS / reverse proxy; align `Jwt:Issuer` / `Audience` with public URLs if you change hostnames.
3. **`Jwt:SigningKey`** — Use a long random secret; never commit production secrets. Production startup **throws** if dev placeholder strings remain.
4. **`IdentitySeed:RunOnStartup` / `BootstrapAdmin`** — Production template disables bootstrap admin; first admin may need a **controlled** enable + password, then disable.
5. **Connection string** — SQL Server connectivity, firewall, and `TrustServerCertificate` (dev convenience) — prefer proper trust in production.
6. **CORS** — If the SPA is served from another origin, set `Cors:AllowedOrigins` to that origin (scheme + host + port).
7. **Seeding** — `RunOnStartup: false` in test/staging clones avoids mutating shared DBs unintentionally.

---

## 4) Release checklist

### Backend

- [ ] `dotnet build` (Release)
- [ ] `dotnet test`
- [ ] Apply EF migrations: `dotnet ef database update --project src/TalentSystem.Persistence --startup-project src/TalentSystem.Api`
- [ ] Set **Production** `appsettings` / environment variables: `Jwt:SigningKey`, `ConnectionStrings:TalentDb`, optional `Cors:AllowedOrigins`
- [ ] Confirm `IdentitySeed` flags for the environment (bootstrap once if needed)
- [ ] Smoke: `GET /api/system/health` (200, no auth)
- [ ] Smoke: `POST /api/auth/login` (valid + invalid)
- [ ] Smoke: one authenticated call per critical module (employees, intelligence generate if permitted, notification template list)

### Frontend

- [ ] `npm run build`
- [ ] `npm run lint`
- [ ] Set `NEXT_PUBLIC_API_BASE_URL` in the deployed environment
- [ ] Login → dashboard → logout / expired token redirect
- [ ] Intelligence page with `INTELLIGENCE_*` permissions (view / generate / manage)

### Cross-cutting

- [ ] Verify **admin** (or HR) role has required permissions after seed
- [ ] Document **first-time** admin creation procedure for production

---

## 5) Material changes (folder tree)

```
src/TalentSystem.Api/
  Program.cs
  appsettings.json
  appsettings.Production.json   (new)
  Controllers/SystemController.cs

src/TalentSystem.Application/
  Features/Identity/Services/AuthService.cs
  Features/Identity/Services/IdentityDatabaseSeeder.cs
  Features/Intelligence/Services/IntelligenceService.cs

talent-web/src/
  stores/auth-store.ts
  components/auth/login-form.tsx

docs/
  PRODUCTION_READINESS.md       (new)
```

---

## 6) Commands

```powershell
# Backend
Set-Location "D:\samier\enterprise Talent Management System"
dotnet build
dotnet test
dotnet ef database update --project src/TalentSystem.Persistence --startup-project src/TalentSystem.Api

# Frontend
Set-Location "D:\samier\enterprise Talent Management System\talent-web"
npm run build
npm run lint
```

---

*Generated as part of Phase 7 — Final Production Checklist / Hardening Pass.*
