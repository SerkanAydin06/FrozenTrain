---
name: frozen-train-work
description: Efficient workflow for FrozenTrain Unity gameplay, survival, UI, interaction, atmosphere, and bug-fix tasks. Use for normal project coding work that does not primarily redesign the train layout.
---
# FrozenTrain work

Use a narrow evidence-first workflow.

1. Run `git status --short`.
2. Identify the exact subsystem from the request.
3. Search exact symbols/filenames first; do not inventory the whole repo.
4. Read only the smallest relevant set of source files, normally 2–5.
5. Reuse existing state/components before adding new managers or parallel systems.
6. Make one coherent patch.
7. Run `git diff --check`, `git diff --stat`, then inspect only the changed diff.
8. Search changed symbols for obvious broken references if renaming/removing anything.

## Routing
- Survival/resource logic: start at `Assets/Scripts/Management/TrainPrototypeState.cs`.
- Simulation timing: `Assets/Scripts/Management/SurvivalSimulation.cs`.
- Management UI: `Assets/Scripts/Management/TrainManagementPanel.cs`.
- FPS interaction: `Assets/Scripts/Interaction/`.
- Player feel/movement: `Assets/Scripts/Player/`.
- Atmosphere only: `Assets/Scripts/Atmosphere/` plus scene-authoring code if the object is authored.

## Cost controls
- Do not use subagents unless the task is genuinely cross-cutting and the user asks for deeper review.
- Do not read generated Unity directories.
- Do not dump full large files when symbol/range reads are sufficient.
- Do not browse external docs for ordinary project-local changes.
- Do not run repeated verification after every tiny edit; verify once after the coherent patch.

If Unity itself cannot be launched, verify statically and state that limitation.