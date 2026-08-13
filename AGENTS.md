# AGENTS.md

Cross-agent entry point for working in this repository.

## Canonical guidance

- Read and follow `.github/copilot-instructions.md` for repository architecture, build and test commands, generated-file rules, and engineering conventions.
- Apply every relevant path-specific file under `.github/instructions/` to the files being changed.
- If a nested `AGENTS.md` is added later, its instructions take precedence within that directory tree.

## Universal rules

- Be concise, direct, and candid. Challenge weak assumptions and distinguish verified facts from uncertainty.
- Keep changes focused and simple. Avoid unrelated edits, unnecessary abstractions, and low-signal tests.
- Make the smallest safe change that fully solves the task and follow existing local patterns.
- Do not include unrelated refactors, formatting-only changes, or incidental generated-file churn.
- Do not hand-edit generated frontend assets or generated `.cshtml` output; edit the corresponding source or `*.Template.cshtml` file.
- Treat `*.resx` files as the localization source of truth; generated locale JSON files are derivatives.
- Preserve existing line endings and validate changes with the smallest relevant build or test command.
