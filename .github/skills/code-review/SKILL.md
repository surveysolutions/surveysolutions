---
name: code-review
description: 'Review Survey Solutions pull requests and diffs for logic, security, data integrity, performance, and architectural regressions. Use for every pull request review, code review, branch diff review, commit review, or review of staged or uncommitted changes.'
---

# Survey Solutions Code Review

## Goal

Find actionable defects introduced by the diff. Prioritize correctness, security, data integrity, concurrency, and performance. Do not produce style-only feedback or findings about unchanged code.

Repository and path-specific custom instructions are authoritative for severity, output format, false-positive exceptions, and technology-specific rules. In particular, apply:

- `.github/copilot-instructions.md`
- `.github/instructions/code-review.instructions.md`
- Every `.github/instructions/*.instructions.md` file whose `applyTo` pattern matches the reviewed file

## Review Workflow

1. Establish the exact base and head revisions and inspect the complete diff. Review added and changed lines only, including deleted behavior when its removal creates a regression.
2. Read the pull request description. When it references a linked issue, incident, or external requirement available through a configured MCP server, retrieve only the relevant item and use its acceptance criteria as intended behavior. Verify that intent against the code rather than assuming the external description is correct.
3. Classify each changed area before evaluating individual lines:
   - API, controller, authentication, authorization, antiforgery, or HTML-rendering boundary
   - Domain command, aggregate, persisted event, event handler, denormalizer, or read model
   - Entity, ORM mapping, schema, migration, transaction, locking, or workspace routing
   - Dependency registration, hosted process, background job, or service boundary
   - Vue route, store, API call, localization resource, or generated frontend output
   - Test, build, deployment, or configuration behavior
4. Follow the changed behavior to the nearest directly affected callers and consumers. Read only enough unchanged code to determine whether the new behavior is safe and complete.
5. For command, event-sourcing, or denormalizer changes, also use the `cqrs-event-flow-review` skill.
6. Check cross-file completeness. Examples include a schema change plus its migration, a new service plus module registration, a new route plus provider or router wiring, a source template plus generated output, and meaningful behavior plus focused tests.
7. Use existing tests, workflow results, and the narrowest relevant build or test command to resolve uncertainty. Do not infer that a successful build proves behavioral correctness.
8. Report only defects that meet the severity threshold and format in `.github/instructions/code-review.instructions.md`. Consolidate repeated instances of the same root cause into one finding.

## MCP Usage

- Use the GitHub MCP server for linked issues, pull request metadata, or failing workflow runs when that context can decide whether changed behavior is correct.
- Use other configured MCP servers only when the pull request explicitly references an item they contain or the custom instructions require that context.
- Do not broaden the review into unrelated issues, incidents, logs, or repository history.
- If MCP context is unavailable, continue reviewing the code and state only assumptions that materially affect a finding.

## Review Boundaries

- Do not modify code while acting as a reviewer.
- Do not flag generated-file hash churn from a legitimate build.
- Do not require a test, migration, handler, or registration merely by convention; establish that the changed behavior needs it.
- Do not report a possible downstream effect without tracing a concrete call, data, persistence, or event path.