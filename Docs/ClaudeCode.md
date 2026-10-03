# FrozenTrain — Claude Code usage

This file is not auto-loaded. It exists as a cheap reference when needed.

## Lowest-cost daily setup

- Use **Sonnet** for normal implementation work.
- Use **Haiku** for trivial renames, tiny lookups, or mechanical edits when available.
- Use **Opus** only for genuinely difficult debugging or architectural decisions.
- Run `/clear` when switching to an unrelated task; old conversation history is resent on later turns.
- Use `/compact` when the same task has become long but you still need its context.
- Prefer file paths and symbol names over pasting large source files/logs into chat.

## Project skills

- `/frozen-train-work` — normal gameplay/survival/UI/interaction/bug work. Usually auto-selected when relevant.
- `/train-layout` — train dimensions, decks, rooms, ladders, connectors, windows, technical plant and layout work. Usually auto-selected when relevant.
- `/verify-frozen-train` — manual low-cost final static verification before commit/PR. It is hidden from automatic invocation to avoid startup/automatic work overhead.

## Efficient prompting

Good:

`Generator gücü kapalıyken hız hâlâ artıyor. TrainPrototypeState ve ilgili UI'yi düzelt, başka sistemlere dokunma.`

`3. vagon üst yatak alanındaki koridoru 20 cm genişlet. /train-layout`

Avoid vague prompts such as `projeyi kontrol et ve düzelt`; they encourage broad exploration.

## Repository setup

- Root `CLAUDE.md` contains only always-needed project facts and cost rules.
- `.claude/rules/` contains path-scoped instructions that load only for matching files.
- `.claude/skills/` contains task procedures loaded on demand.
- `.claude/settings.json` limits read output/concurrency, denies generated Unity directories, pre-approves safe git inspection, and runs deterministic `git diff --check` after edits.

## Unity workflow

For authored train geometry changes, after pulling locally:

1. Let Unity finish compiling.
2. Run `Frozen Train > Rebuild Authored Train` once.
3. Enter Play mode and validate the changed area.

Do not claim Unity compilation/runtime verification unless Unity was actually run.