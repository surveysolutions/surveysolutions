# AGENTS.md

Guidance for coding agents working in this repository.

## Repository at a glance

- Product: **Survey Solutions** (survey management and data collection platform)
- Main stack: **.NET 9 / C#**, **Vue 3**, **PostgreSQL**
- Frontends:
  - `src/UI/WB.UI.Frontend` — Headquarters SPA + WebInterview (**Vuex 4**, Axios)
  - `src/UI/WB.UI.Designer` — Designer SPA (**Pinia**, Mande, Vuetify 3)
- Backend structure:
  - `src/Core/BoundedContexts/` — domain logic by bounded context
  - `src/Core/SharedKernels/` — shared domain concepts
  - `src/Infrastructure/` — NHibernate, migrations, persistence
  - `src/Services/Export/` — separate export microservice using **EF Core**
  - `src/Tests/` — unit, web, integration, designer, export, scheduler tests

## Important architecture rules

- Follow the existing bounded-context structure; keep changes local to the touched area.
- Headquarters uses **CQRS/Event Sourcing** patterns. Prefer read-side repositories/services for queries.
- Do **not** read domain state through `IUnitOfWork.Session` in application services unless a provider limitation makes it unavoidable.
- Register new backend services in the appropriate `*Module` class, not in `Startup.cs`.
- Use constructor injection; avoid service locator in application code.
- Workspace tenancy is schema-based (`ws_{workspace}`); use `IWorkspaceContextAccessor`, never hardcode schema names.
- Export service is separate from Headquarters and uses **EF Core**, not NHibernate.

## Backend conventions

- Target runtime is **.NET 9** (`global.json` pins SDK `9.0.203`; CI uses `9.0.x`).
- Many projects treat warnings as errors; new nullable issues will often fail the build.
- Prefer existing abstractions and patterns over introducing new infrastructure.
- New schema changes require a matching migration:
  - Headquarters / Designer: **FluentMigrator**
  - Export service: **EF Core migrations**
- Most meaningful C# production changes should include or update unit tests in the relevant test project.

## Frontend conventions

### `src/UI/WB.UI.Frontend`

- Use **Vuex 4** for state management; do not introduce Pinia here.
- Use **Axios** and existing service helpers instead of raw `fetch`.
- User-visible strings should use localization, not hardcoded English.
- Any HTML rendered from dynamic content must be sanitized (`vue-dompurify-html` / DOMPurify).
- New routes should follow the existing component-provider wiring instead of ad-hoc registration.

### `src/UI/WB.UI.Designer`

- Use **Pinia** for new state management; do not add new Vuex modules.
- Use existing API helpers from `questionnaire/src/services/apiService.js` rather than raw `fetch`.
- Use **Vuetify 3** APIs only.
- Sanitize dynamic HTML before rendering.
- Register routes centrally in `questionnaire/src/router/index.js`.

## Generated files and build output

- Do **not** hand-edit generated frontend assets or generated `.cshtml` build outputs.
- If a generated `.cshtml` file needs a real change, edit its corresponding `*.Template.cshtml` source instead.
- Localization source of truth is `*.resx`; generated JSON resource files are derivatives.
- Building frontend apps can modify generated artifacts such as locale JSON, `package-lock.json`, and some `.cshtml` outputs; avoid committing incidental build churn.

## Validation commands

### General backend

```bash
dotnet build src/WB.sln
dotnet test src/Tests/WB.Tests.Unit
dotnet test src/Tests/WB.Tests.Web
dotnet test src/Tests/WB.Tests.Unit.Designer
```

### Integration / service tests

These require PostgreSQL and local config files similar to CI:

```bash
dotnet test src/Tests/WB.Tests.Integration
dotnet test src/Tests/WB.Tests.Integration.Designer
dotnet test src/Services/Export/WB.Services.Export.Tests
dotnet test src/Services/Core/WB.Services.Scheduler.Tests
```

### Frontend

`WB.UI.Frontend`:

```bash
cd src/UI/WB.UI.Frontend
npm install
npm run build
npm test
npm run lint
```

`WB.UI.Designer`:

```bash
cd src/UI/WB.UI.Designer
npm install
npm run build
```

CI workflows currently use **Node 24** for builds.

## Security and correctness checks

- Add explicit authorization attributes on new backend endpoints.
- Treat `@Html.Raw(...)` and `v-html` as dangerous unless the content is trusted or sanitized.
- Include CSRF tokens on mutating browser requests unless the endpoint explicitly opts out.
- Avoid `console.log` in production frontend code.
- Be careful with concurrency-sensitive write paths; follow existing locking patterns where they already exist.

## Practical working rules

- Make the smallest safe change that fully solves the task.
- Do not include unrelated refactors, formatting-only edits, or generated-file churn.
- Preserve existing line endings and file organization.
- When editing C# or frontend code, inspect the relevant local conventions in `.github/instructions/`.
