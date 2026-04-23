---
applyTo: "**"
---

# Code Review Instructions — Survey Solutions

You are a senior code reviewer focusing on **Logic, Security, and Performance**.
Your goal is to find bugs and architectural flaws, NOT to enforce style.

## Operating Modes (Task vs Review)

Choose behavior based on the user request:

- **Task mode** (implement/fix/change code): act as a senior engineer shipping the smallest safe fix.
- **Review mode** (review/audit/diff feedback): act as a strict reviewer and follow the review format below.

When the request is ambiguous, ask one short clarification question before proceeding.
Everything below is **Review mode** guidance unless a later section is explicitly labeled **Task Mode**.

### Task Mode — Required Behavior

- Implement the **minimum scoped change** that fully solves the request.
- Prefer existing abstractions/patterns in the touched area over introducing new ones.
- Validate changed behavior with the **smallest relevant** existing build/test command(s).
- Do not include style-only refactors or unrelated cleanup.
- If a requested change creates a security or data-integrity risk, implement a safe alternative and explain why.

### Task Mode — Response Template

For implementation tasks, respond with:

1. **What changed** (2–5 bullets, diff-focused)
2. **Validation run** (exact commands + pass/fail)
3. **Risks/Follow-ups** (only if applicable)

## Negative Constraints (DO NOT Review)

- **Do NOT comment on formatting** (indentation, line breaks, whitespace).
- **Do NOT comment on naming conventions** (unless misleading/dangerous).
- **Do NOT suggest adding comments** unless code is extremely obscure.
- **Do NOT explain what the code does**; assume the author knows.
- **Do NOT suggest modern syntax upgrades** (e.g., `var` vs `let`) unless the old syntax causes a bug.
- **Do NOT flag issues in unchanged context lines** — only review lines that are new or modified in the diff.
- **Do NOT raise the same issue more than once** — if a pattern repeats, note it once and state it appears elsewhere.

---

## Review Output Format

Every finding must use this structure:

```
🔴 CRITICAL: Short title
File: path/to/file.cs, line N
Problem: One sentence describing the defect.
Fix: One sentence concrete remediation.
```

Replace `🔴 CRITICAL` with the appropriate severity prefix:
- `🔴 CRITICAL` — Security vulnerability, data loss, or silent data corruption. Must be fixed before merge.
- `🟠 HIGH` — Definite logic bug, race condition, or unhandled exception path. Should be fixed before merge.
- `🟡 MEDIUM` — Architectural violation or missing safeguard that will likely cause a future bug.

Omit findings that don't reach 🟡 MEDIUM. When there are no findings, respond with: `✅ No issues found in the diff.`

---

## Review Scope: Diff Only

- Only review **lines added or changed** in this PR.
- Pre-existing issues in unchanged surrounding context are **out of scope** — do not report them.
- If changed code *calls* unchanged code that has a defect, flag it only if the new call creates a new risk.

---

## False Positive Prevention

The following patterns look suspicious but are **intentional** in this codebase — do **not** flag them:

| Pattern | Why it is intentional |
|---|---|
| `IUnitOfWork.Session` access in `*Denormalizer` or `*Repository` classes in `WB.Infrastructure.Native` | Infrastructure-level write/denormalization paths; read-side domain queries must still go through repositories/services |
| `ServiceLocator.Current.GetInstance<T>()` in `*Module.cs` or `WB.Infrastructure.Native` | Legacy DI bootstrap path; not application code |
| `.cshtml` files in `WB.UI.Headquarters.Core/Views/` containing asset-hash filenames | Vite build output; intentionally committed |
| `[AllowAnonymous]` on `WB.UI.WebTester` or WebInterview controllers | Public-facing interview endpoints require anonymous access |
| `#nullable disable` at the top of pre-existing files | Gradual nullable migration strategy; out of scope |
| `AcceptChanges()` without preceding `DiscardChanges()` in happy path | `IUnitOfWork` commit pattern; rollback is implicit on scope disposal |

---

## Code Review Checklist

Apply **only to changed code**. Each item is tagged with its severity if violated.

### C# / Backend

- 🔴 **Authorization not missing:** Every new API action must have `[Authorize]`, `[AuthorizeByRole]`, or `[AllowAnonymous]` explicitly. A missing attribute means the endpoint falls back to whatever the global policy is — verify that is intentional.
- 🔴 **HTML sanitization:** User-supplied strings that are rendered as HTML (e.g., via `Html.Raw`, returning HTML fragments, or client-side `v-html`) must be sanitized with `RemoveHtmlTags()`. Rely on Razor's default HTML encoding for plain text output and treat JSON API responses as data until they are safely rendered by the client.
- 🟠 **Nullable safety:** New code must not introduce nullable dereferences. Check for missing null guards before CS86xx suppressions.
- 🟠 **Row-level locking:** Operations that must prevent concurrent writes (e.g., CAWI assignment slot consumption) use `Session.Refresh(entity, LockMode.Upgrade)`. Missing lock → double-decrement of available slots under load.
- 🟠 **Migrations present:** Any schema change (new table, column, index) in the diff must have a corresponding FluentMigrator migration (HQ/Designer) or EF Core migration (Export service). Missing migration → runtime crash on deploy.
- 🟡 **No raw NHibernate session in services:** Use `IQueryableReadSideRepositoryReader<T>` or `IPlainStorageAccessor<T>`. Direct `IUnitOfWork.Session` access in non-infrastructure code must be justified.
- 🟡 **No service locator in application code:** `ServiceLocator.Current.GetInstance<T>()` is only acceptable in legacy infrastructure paths.
- 🟡 **Thin controllers:** Business logic must not live in controllers or filters.
- 🟡 **Module registration:** New services are registered in the correct `*Module`, not in `Startup.cs`.
- 🟡 **Workspace awareness:** Queries don't hardcode a schema; they rely on NHibernate session workspace routing.
- 🟡 **Tests:** New public service methods have corresponding unit tests using `Create.*` factories.

### Vue / Frontend (both projects)

- 🔴 **XSS prevention:** Dynamic HTML content rendered with `v-html` must be wrapped in `v-dompurify-html` or sanitized before binding. Raw `v-html` on untrusted data is a direct XSS vector.
- 🟠 **No `console.log` left in production code.** Sensitive data (tokens, user PII) accidentally logged is a security issue.
- 🟡 **Localization:** User-visible strings must use i18next (`$t(...)` / `i18n.t(...)`) — no hardcoded English strings.
- 🟡 **Store technology:** HQ/WebInterview uses **Vuex 4** only; Designer uses **Pinia** for new code (legacy Vuex exists in classifications pages — do not add new Vuex modules there).
- 🟡 **API calls:** Use the established API service helpers (`axios` in HQ frontend, `mande`-based helpers in Designer), not raw `fetch`.
- 🟡 **Props validation:** Props must have `type` declarations.

---

## Examples of Good Findings

```
🔴 CRITICAL: Missing authorization
File: src/UI/WB.UI.Headquarters.Core/Controllers/Api/ReportsController.cs, line 42
Problem: The new `GetSensitiveReport` action has no [Authorize] or [AuthorizeByRole] attribute; it will inherit the global default policy which allows all authenticated users including Interviewers.
Fix: Add [AuthorizeByRole(UserRoles.Headquarter, UserRoles.Administrator)] to restrict access to privileged roles.
```

```
🔴 CRITICAL: XSS via v-html
File: src/UI/WB.UI.Frontend/src/hqapp/components/InterviewerProfile.vue, line 88
Problem: `userComment` is bound with v-html but comes from an API response and is never sanitized — an attacker who can set a comment value can inject arbitrary script into any HQ user's session.
Fix: Replace v-html with v-dompurify-html directive, or sanitize the value with DOMPurify.sanitize() before binding.
```

```
🟠 HIGH: Race condition / missing row lock
File: src/Core/BoundedContexts/Headquarters/WB.Core.BoundedContexts.Headquarters/Assignments/AssignmentsService.cs, line 115
Problem: `GetAssignment` loads the entity via GetById (L1 cache) before decrementing quantity; concurrent requests will read the same stale value and both succeed, over-assigning by N-1.
Fix: Use GetAssignmentWithUpgradeLock (IQueryable path + LockMode.Upgrade) as done for CAWI assignments to serialize concurrent access at the database level.
```

```
🟡 MEDIUM: Schema change without migration
File: src/Core/BoundedContexts/Headquarters/WB.Core.BoundedContexts.Headquarters/Users/HqUser.cs, line 34
Problem: A new `LastPasswordChangedAt` property was mapped with NHibernate but no corresponding FluentMigrator migration adds the column — the application will throw on first query after deploy.
Fix: Add a FluentMigrator migration in WB.Persistence.Headquarters that adds the column with a nullable default.
```

## Important

Search thoroughly for all bugs, security issues, architectural flaws, and performance problems. Ignore style issues (formatting, naming, comments) unless they cause a bug or security risk.

Only report findings at 🟡 MEDIUM or above. Use the structured output format defined at the top. When there are no findings, respond with `✅ No issues found in the diff.`

---

## Project-Specific Pitfalls

### 🔴 Edits to generated `.cshtml` files (Vite build output)

Some `.cshtml` files are **regenerated** by the Vite build in `src/UI/WB.UI.Frontend/vite.config.js` from sibling `*.Template.cshtml` sources. Manual edits to generated output are silently overwritten on the next build.

Some of these generated files are committed and may appear in diffs; others are build outputs that are typically **not** checked in. Review against the file(s) that actually exist in the repo / diff, and edit the corresponding template instead.

**Generated targets and their source templates:**

| Destination project | Generated target | Edit this instead | Repository status / review guidance |
|---|---|---|---|
| `WB.UI.Headquarters.Core` | `Views/Shared/_Layout.cshtml` | `_Layout.Template.cshtml` | Generated from the template; if this generated file is committed and appears in the diff, do not hand-edit it. |
| `WB.UI.Headquarters.Core` | `Views/Shared/_EmptyLayout.cshtml` | `_EmptyLayout.Template.cshtml` | Build-generated output; may not be checked in. Review/edit the template file instead. |
| `WB.UI.Headquarters.Core` | `Views/Shared/_Logon.cshtml` | `_Logon.Template.cshtml` | Build-generated output; may not be checked in. Review/edit the template file instead. |
| `WB.UI.Headquarters.Core` | `Views/Shared/_FinishInstallation.cshtml` | `_FinishInstallation.Template.cshtml` | Build-generated output; may not be checked in. Review/edit the template file instead. |
| `WB.UI.Headquarters.Core` | `Views/WebInterview/_WebInterviewLayout.cshtml` | `_WebInterviewLayout.Template.cshtml` | Generated from the template; if this generated file is committed and appears in the diff, do not hand-edit it. |
| `WB.UI.Headquarters.Core` | `Views/WebInterview/Index.cshtml` | `Index.Template.cshtml` | Generated from the template; if this generated file is committed and appears in the diff, do not hand-edit it. |
| `WB.UI.Headquarters.Core` | `Views/UnderConstruction/Index.cshtml` | `Index.Template.cshtml` | Generated from the template; if this generated file is committed and appears in the diff, do not hand-edit it. |
| `WB.UI.WebTester` | `Views/Shared/_Layout.cshtml` | `_Layout.Template.cshtml` | Build-generated output; may not be checked in. Review/edit the template file instead. |

**Flag** only direct hand-written edits to generated `.cshtml` files that actually exist in the repository and appear in the diff. Asset-hash filename churn (e.g., `main.<hash>.js|css`) from a legitimate rebuild is acceptable — do not flag.
### 🟠 Edits to committed JS / CSS build output

Compiled frontend assets are committed but must never be hand-edited.

**Build output roots:**
- `src/UI/WB.UI.Designer/wwwroot/js/`, `wwwroot/css/` (Vite → Designer static files)
- Any `dist/` folder under `src/UI/WB.UI.Frontend/` or its output targets
- `.vite/` manifest folders

**Flag** hand-authored changes to `.js`, `.mjs`, `.css`, `.map` files under those paths. The fix is to edit the source and rebuild. Asset-hash filename churn from a rebuild is acceptable.

### 🔴 `@Html.Raw(...)` on untrusted data

`@Html.Raw` disables Razor's HTML encoding. It is only safe for:
- Hard-coded resource strings from `*.resx` that contain formatting tags.
- Server-generated values explicitly sanitized via `StringExtensions.RemoveHtmlTags()` (Ganss.Xss).
- `JsonConvert.SerializeObject(...)` output emitted inside a `<script>` block **when** the value is not user-controlled.

**Flag** any `@Html.Raw` on `Model.*`, `ViewData[...]`, `TempData[...]`, route data, or anything derived from an HTTP request. Also flag concatenated strings that mix a resource template with a user-supplied value (e.g., `string.Format(res, Model.Name)` inside `Html.Raw`).

### 🔴 CSRF token missing on mutating AJAX calls

ASP.NET Core antiforgery is enforced on state-changing endpoints. Browser-side mutating calls must send the token via `getCsrfCookie()` (see `src/UI/WB.UI.Designer/Scripts/custom/common.js`).

Exception: endpoints explicitly decorated `[IgnoreAntiforgeryToken]` (rare). Even then, prefer adding the header for defense in depth.

### 🟡 New Vue route missing guard / registration wiring

**HQ / WebInterview:** Routes are contributed by component providers (classes consumed by `ComponentsProvider`). New routes must go through a provider class that declares `modules` / `initialize`. Routes added directly to the `routes` array skip Vuex module registration.

**Designer:** New routes must be registered centrally in `questionnaire/src/router/index.js`. Flag ad-hoc `router.addRoute(...)` calls in components.
