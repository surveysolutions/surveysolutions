# Copilot Instructions — Survey Solutions

## Repository Overview

**Survey Solutions** is a large-scale, production-grade survey management and data collection platform developed by the World Bank. The repository contains multiple .NET backend projects, two Vue 3 frontends, export/scheduler microservices, integration and unit tests, database migrations, and deployment tooling.

- **Primary Languages:** C# (.NET 9), JavaScript/Vue 3 (frontend)
- **Database:** PostgreSQL (NHibernate ORM for HQ, EF Core for Export service)
- **Target Runtimes:** .NET 9, Node.js 22 (frontend; match CI workflow `NODE_VERSION`)

---

## Architecture and Design Patterns

### Domain-Driven Design with Bounded Contexts

The backend is organized into bounded contexts under `src/Core/BoundedContexts/`:

| Bounded Context | Project | Purpose |
|---|---|---|
| `Headquarters` | `WB.Core.BoundedContexts.Headquarters` | Main server-side domain: users, assignments, interviews, questionnaires |
| `Designer` | `WB.Core.BoundedContexts.Designer` | Questionnaire authoring |
| `Interviewer` | `WB.Core.BoundedContexts.Interviewer` | Mobile interviewer domain |
| `Supervisor` | `WB.Core.BoundedContexts.Supervisor` | Mobile supervisor domain |
| `Tester` | `WB.Core.BoundedContexts.Tester` | Questionnaire testing |

Shared domain concepts live in `src/Core/SharedKernels/` (DataCollection, Enumerator, Questionnaire).

### CQRS and Event Sourcing (Ncqrs)

- **Write side:** Aggregate roots extend `EventSourcedAggregateRoot` (Ncqrs), publishing domain events. Commands are dispatched through `ICommandService` (the command bus).
- **Read side:** Denormalizers consume events and persist read models. Read models are accessed via `IQueryableReadSideRepositoryReader<TEntity, TKey>`.
- Key aggregate: `Interview` (`src/Core/SharedKernels/DataCollection/DataCollection/Implementation/Aggregates/Interview.cs`)

> **Review rule:** Never access `IUnitOfWork.Session` directly for reading domain state — use the appropriate read-side repository or service abstraction. Direct session queries are only justified when a LINQ provider limitation requires it (document with a comment why).

### Dependency Injection — Custom Module System

Registration uses a custom `IModule` / `IIocRegistry` abstraction (wrapping Autofac) defined in `WB.Core.Infrastructure`. Each bounded context exposes a `*Module : IModule` class (e.g., `HeadquartersBoundedContextModule`).

- Register all new services inside the corresponding `*Module.Load(IIocRegistry)`.
- Do **not** register services directly in `Startup.cs` unless they are infrastructure-level concerns.

### UnitOfWork and NHibernate

- `IUnitOfWork` (`WB.Infrastructure.Native.Storage.Postgre`) wraps an NHibernate `ISession`.
- The session is workspace-aware: each workspace maps to its own PostgreSQL schema (`ws_{name}`).
- `IUnitOfWork` must **not** be resolved in root (singleton) scope — the infrastructure enforces this and logs an error.
- Call `AcceptChanges()` explicitly to commit; `DiscardChanges()` to roll back within a scope.

### Workspace Multi-Tenancy

- Each HQ workspace has its own schema: `ws_{workspaceName}` (e.g., `ws_primary`).
- Workspace context is provided by `IWorkspaceContextAccessor`.
- All repository/session access automatically routes to the current workspace schema.
- Always inject `IWorkspaceContextAccessor` rather than hard-coding schema names.

### Export Service

`src/Services/Export/` is a **separate microservice** with its own technology stack:
- Uses **EF Core + Npgsql** (not NHibernate).
- Has its own EF Core migrations (`WB.Services.Export/Migrations/`).
- Communicates with HQ via HTTP (not in-process).

### Database Migrations

| Project | Migration Tool |
|---|---|
| `WB.Persistence.Headquarters` | FluentMigrator |
| `WB.Persistence.Designer` | FluentMigrator |
| `WB.Services.Export` | EF Core Migrations |

---

## Build and Test Commands

### Backend

```bash
# Build entire solution
dotnet build "src/WB.sln"

# Unit tests
dotnet test src/Tests/WB.Tests.Unit
dotnet test src/Tests/WB.Tests.Web
dotnet test src/Tests/WB.Tests.Unit.Designer

# Integration tests (requires PostgreSQL)
dotnet test src/Tests/WB.Tests.Integration
dotnet test src/Tests/WB.Tests.Integration.Designer
dotnet test src/Services/Export/WB.Services.Export.Tests
dotnet test src/Services/Core/WB.Services.Scheduler.Tests
```

### Frontend — WB.UI.Frontend (HQ + WebInterview)

```bash
cd src/UI/WB.UI.Frontend
npm install
npm run build           # production build
npm run dev             # development build (one-off, Vite build --mode development)
npm run hot             # dev server with HMR (Vite)
npm test                # Jest unit tests
npm run lint            # ESLint
npm run format          # Prettier
```

### Frontend — WB.UI.Designer

```bash
cd src/UI/WB.UI.Designer
npm install
npm run build           # production build (outputs to wwwroot)
npm run dev             # development build (non-watch, Vite build --mode development)
npm run hot             # Vite build --watch (continuous build)
npm run hot2            # Vite dev server (HMR)
```

---

## Project Layout Reference

```
src/
  Core/
    BoundedContexts/          # Domain logic per bounded context
      Designer/
      Headquarters/
      Interviewer/
      Supervisor/
      Tester/
    SharedKernels/            # Shared domain concepts
      DataCollection/         # Interview aggregate, commands, events
      Enumerator/
      Questionnaire/
    Infrastructure/
      WB.Core.Infrastructure/ # CQRS infrastructure, IModule, IUnitOfWork interface
  Infrastructure/
    WB.Infrastructure.Native/ # NHibernate/Postgres IUnitOfWork impl, WorkspaceContext
    WB.Persistence.Headquarters/  # FluentMigrator migrations (HQ)
    WB.Persistence.Designer/      # FluentMigrator migrations (Designer)
  Services/
    Export/
      WB.Services.Export/     # Export microservice (EF Core)
      WB.Services.Export.Host/
    Core/
      WB.Services.Scheduler/  # Quartz.NET job scheduler
  UI/
    WB.UI.Headquarters.Core/  # ASP.NET Core HQ web app
    WB.UI.Designer/           # ASP.NET Core Designer web app
      questionnaire/          # Vue 3 SPA (Pinia, Vuetify)
    WB.UI.Frontend/           # Vite project → HQ SPA + WebInterview (Vuex)
    WB.UI.WebTester/          # ASP.NET Core web tester
  Tests/
    WB.Tests.Abc/             # Shared test factories and helpers
    WB.Tests.Unit/
    WB.Tests.Web/
    WB.Tests.Unit.Designer/
    WB.Tests.Integration/
    WB.Tests.Integration.Designer/
```

---

## Common Pitfalls

- **UnitOfWork in root scope:** Resolving `IUnitOfWork` as a singleton causes `ObjectDisposedException` — always resolve it in a request or job scope.
- **NHibernate L1 cache vs. locking:** Before acquiring a row-level lock with `LockMode.Upgrade`, bypass the L1 cache by querying via `IQueryable` (not `GetById`). See `AssignmentsService.GetAssignmentWithUpgradeLock`.
- **Vite output files are committed:** The Vite build in `WB.UI.Frontend` writes asset-hashed filenames directly into `.cshtml` Razor templates inside `WB.UI.Headquarters.Core/Views`. These generated files **must** be committed.
- **Integration tests need config:** Set `appsettings.cloud.ini` / `appsettings.cloud.json` with PostgreSQL credentials before running integration tests locally (mirrors CI environment variables).
- **Designer uses Vuetify 3:** Do not import Vuetify 2 components or use Vuetify 2 API (`v-data-table` slot names differ between versions, etc.).
- **Workspace schema isolation:** Running ad-hoc SQL against a specific workspace requires prefixing table names with the workspace schema (e.g., `ws_primary."interviews"`).
