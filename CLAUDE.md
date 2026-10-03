# FrozenTrain — Claude Code Guide

## Goal
Work on the existing Unity survival-management game efficiently. Prefer focused edits over broad exploration.

## Project facts
- Engine: Unity 6.3 LTS, URP, Windows.
- Main scene: `Assets/Scenes/FPSPrototype.unity`.
- Exactly 5 cars: Locomotive, Generator, Living, Workshop, Greenhouse.
- Do not add cars or story/NPC narrative unless explicitly requested.
- Current authored footprint: 18.0 m x 9.2 m per car.
- Train has 3 full-size decks:
  - lower: technical infrastructure only;
  - middle: occupied/social/work/production;
  - upper: sleeping/accommodation with a central inter-car corridor.
- Locomotive front is -X. Exterior scenery moves +X.
- Public authoring workflow: `Frozen Train > Rebuild Authored Train`.

## Source of truth
- Final train layout: `Assets/Editor/FrozenTrainFinalThreeDeckAuthoring.cs`
- Rebuild entry point: `Assets/Editor/FrozenTrainUnifiedAuthoring.cs`
- Size/placement normalization: `Assets/Editor/FrozenTrainAuthoredLayoutResize.cs`
- Physical interaction binding: `Assets/Editor/FrozenTrainPhysicalSystemsAuthoring.cs`
- Architecture contract: `Docs/FiveCarArchitecture.md`
- Survival state: `Assets/Scripts/Management/TrainPrototypeState.cs`
- Simulation: `Assets/Scripts/Management/SurvivalSimulation.cs`
- Management UI: `Assets/Scripts/Management/TrainManagementPanel.cs`

## Cost / context discipline
- Do not scan the whole repository before starting.
- Start with `git status`, then search exact symbols/filenames and read only relevant files/ranges.
- Do not read generated Unity folders (`Library`, `Temp`, `Logs`, `obj`, `UserSettings`, builds).
- Do not read `.meta` files unless GUID/import behavior is directly relevant.
- For normal tasks, inspect at most 2–5 source files before editing; expand only when evidence requires it.
- Prefer `git grep`/targeted search over recursive full-file reads.
- Do not use subagents for routine edits, lookups, or verification.
- Do not browse the web unless current external information is necessary.
- Keep responses concise: findings, files changed, verification, exact Unity step if required.
- Do not repeatedly explain project history already recorded here.

## Editing rules
- Preserve the existing 5-car architecture unless the user explicitly changes it.
- Visual train geometry is authored in editor/prefab code, not spawned as gameplay geometry at runtime.
- Keep `Frozen Train` menu to one public command: `Rebuild Authored Train`.
- Reuse existing systems and object names before creating parallel systems.
- Avoid decorative clutter that blocks corridors, windows, ladders, doors, or interactions.
- Keep window sight-lines clear in windowed spaces.
- Preserve `E` interaction stability and existing component bindings when moving/replacing authored objects.

## Workflow
1. Read only the source-of-truth file(s) relevant to the request.
2. Make one coherent change instead of many tiny parallel systems.
3. Check `git diff --check` and inspect the changed diff only.
4. If Unity cannot be run locally, say so; do not claim compilation succeeded.
5. If authored geometry changed, tell the user to run `Frozen Train > Rebuild Authored Train` once after pulling.

Use the project skills when their descriptions match the task.