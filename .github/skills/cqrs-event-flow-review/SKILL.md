---
name: cqrs-event-flow-review
description: 'Review Survey Solutions CQRS and event-sourcing changes across commands, command registration, aggregate handlers, persisted events, Apply methods, denormalizers, read models, synchronization, and Export consumers. Use when a diff changes any command, aggregate, domain event, event handler, projection, or denormalizer.'
---

# CQRS and Event Flow Review

## Goal

Verify that a changed domain behavior remains correct from command dispatch through aggregate replay and every required projection or downstream consumer. Apply this workflow in addition to the repository's normal code-review instructions.

Useful implementation anchors include:

- Command registration: `src/Core/BoundedContexts/**/**BoundedContextModule.cs`
- Command infrastructure: `src/Core/Infrastructure/WB.Core.Infrastructure/CommandBus/`
- Main interview aggregate: `src/Core/SharedKernels/DataCollection/DataCollection/Implementation/Aggregates/Interview.cs`
- Headquarters denormalizers: `src/Core/BoundedContexts/Headquarters/WB.Core.BoundedContexts.Headquarters/EventHandler/`
- Export consumers: `src/Services/Export/WB.Services.Export/`
- Shared test factories: `src/Tests/WB.Tests.Abc/`

## Build the Behavior Trace

For each changed behavior, identify the applicable stages. Do not assume every flow uses every stage.

1. **Dispatch and identity**
   - Find the command creation and `ICommandService` dispatch call.
   - Find its `CommandRegistry.Setup<TAggregate>()` registration, normally in the owning bounded-context module.
   - Verify the aggregate ID resolver, `InitializesWith`, `Handles`, or `StatelessHandles` semantics, and configured validators, pre-processors, and post-processors.
2. **Aggregate decision**
   - Trace the registration delegate to the aggregate method.
   - Verify authorization-independent domain invariants, state-dependent guards, idempotency expectations, and concurrency assumptions against neighboring behavior.
   - Confirm each accepted state change emits the intended event through the established `ApplyEvent(...)` pattern.
3. **Persisted event and replay**
   - Treat persisted event types and serialized members as long-lived contracts. Check compatibility when a type, member, default, or meaning changes.
   - Find the aggregate `Apply(TEvent)` method or equivalent replay path and verify that replay reconstructs the same state needed by later commands.
   - Check event ordering when multiple events are emitted by one command and later handlers depend on that order.
4. **Read-side consumers**
   - Search for all usages of each changed event type across Headquarters, Designer, mobile bounded contexts, shared kernels, and services.
   - Verify every projection that promises the changed data is updated correctly; do not require unrelated projections to consume the event.
   - For composite functional handlers, verify the part handler is included in the composite and the composite is registered when required.
   - Check denormalizers for duplicate delivery, retry, ordering, deletion, and missing-entity behavior where the event bus contract makes those cases possible.
5. **Cross-process and synchronization contracts**
   - Check Export service consumers separately because Export uses EF Core and runs out of process.
   - When an event is synchronized to interviewer or supervisor applications, verify serialization and version-tolerant handling in those consumers.
6. **Persistence and registration**
   - Verify new services and denormalizers are registered in the owning `*Module`, not application `Startup` code.
   - If a read model, mapping, table, column, or index changes, verify the corresponding FluentMigrator or EF Core migration exists in the correct project.
   - Preserve workspace-aware access; do not accept hard-coded workspace schemas.
7. **Tests**
   - Look for focused aggregate tests covering rejection and emitted events, replay tests when state reconstruction changes, denormalizer tests for projection updates, and Export tests for cross-process consumers.
   - Use existing `Create.*` factories and neighboring test organization.
   - A missing test is a finding only when the changed behavior creates meaningful unverified risk under the repository review policy.

## High-Risk Defects

Prioritize concrete evidence of:

- A command registered against the wrong aggregate ID or with incorrect initializer/stateless semantics
- State mutated without a replay-equivalent persisted event
- A persisted event contract changed incompatibly with existing event streams
- An `Apply(TEvent)` path that leaves rehydrated state different from live state
- A required read model or Export consumer left stale
- A denormalizer registered twice, not registered, or omitted from its composite
- Non-idempotent projection behavior exposed to retries or duplicate delivery
- A schema-dependent projection change without the correct migration
- A concurrency-sensitive command that reads stale state or bypasses the established locking pattern

## False-Positive Guards

- Not every event needs a consumer in every bounded context.
- `StatelessHandles` and initializer registrations are valid when consistent with the aggregate lifecycle.
- Direct `IUnitOfWork.Session` access is allowed in the infrastructure denormalizer and repository exceptions documented by the review instructions.
- Do not demand migration, synchronization, or Export changes without tracing an actual affected contract.
- Report the root defect once, on a changed line that introduced the incomplete or unsafe flow.