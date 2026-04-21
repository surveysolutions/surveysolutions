---
applyTo: "**/*.cs"
---

# C# Conventions — Survey Solutions

## Code Style

- **Language version:** `latest` — use modern C# features freely (pattern matching, records, switch expressions, etc.).
- **Nullable reference types:** Selected nullability warnings (CS8600–CS8634, CS8714) are treated as errors via `WarningsAsErrors`. New code must be null-safe.
  - Use `#nullable enable` when adding nullable annotations to existing files that predate the policy.
- **Constructor injection:** All dependencies injected via constructor; no service locator in application code (service locator exists only in legacy infrastructure).
- **Interface-first:** Every service class should implement an interface; program to the interface.
- **Reusable predicates:** Extract reusable NHibernate/LINQ filters as `private static Expression<Func<T, bool>>` methods (see `AssignmentsService.cs` for the pattern).
- **Authorization:** Use `[AuthorizeByRole(UserRoles.X)]` on controllers/actions. The `UserRoles` enum values are: `Administrator`, `Supervisor`, `Interviewer`, `Headquarter`, `Observer`, `ApiUser`.
- **HTML sanitization:** Untrusted string values rendered as HTML must be sanitized with `StringExtensions.RemoveHtmlTags()` (uses Ganss.Xss under the hood).

## API Controllers

- API controllers inherit from `ControllerBase` and are annotated with `[Route("api/{controller}/{action=Get}")]`.
- Thin controllers: delegate all logic to domain services; no domain logic in controllers.
- Use `IAuthorizedUser` to resolve the current user identity (not `HttpContext.User` directly).

## Testing Conventions

### Frameworks

| Purpose | Library |
|---|---|
| Test framework | NUnit 4 (`[TestFixture]`, `[Test]`) |
| Assertions | FluentAssertions |
| Mocking | Moq (preferred) or NSubstitute |
| Test data | AutoFixture + custom `Create.*` factories |

### Factory Pattern — `Create.*`

`WB.Tests.Abc` provides static factory entry points:

```csharp
Create.Entity.*          // Domain entities (Assignment, Interview, etc.)
Create.AggregateRoot.*   // Aggregate roots pre-loaded with state
Create.Service.*         // Service implementations
Create.Command.*         // Command objects
Create.Event.*           // Domain events
Create.PublishedEvent.*  // Wrapped events for denormalizer tests
Create.Storage.*         // In-memory repository fakes
```

Use these factories in all unit tests. Do not `new` up domain objects directly unless no factory exists.

### Naming

Test methods follow the pattern: `when_{context}_should_{expectation}` or `when_{action}_and_{condition}`.

Mark the class under test with `[TestOf(typeof(MyService))]`.

### Test Project Responsibilities

| Project | What it tests |
|---|---|
| `WB.Tests.Unit` | Pure unit tests — no database, no I/O |
| `WB.Tests.Web` | ASP.NET Core controller and filter tests |
| `WB.Tests.Unit.Designer` | Designer bounded context unit tests |
| `WB.Tests.Integration` | Requires PostgreSQL — integration tests for HQ |
| `WB.Tests.Integration.Designer` | Requires PostgreSQL — Designer integration tests |
| `WB.Services.Export.Tests` | Export service tests |
| `WB.Services.Scheduler.Tests` | Scheduler service tests |
