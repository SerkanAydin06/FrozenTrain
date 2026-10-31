---
name: train-layout
description: Use for FrozenTrain dimensions, decks, corridors, vestibules, doors, ladders, structural train geometry and placement.
---
# Train layout — focused workflow

Start with **the specific symbol/file** involved; usually 1–3:
- `Assets/Editor/FrozenTrainLayout.cs` — dimensions and deck data.
- `Assets/Editor/FrozenTrainLongVestibuleArchitecture.cs` — final door/circulation authoring.
- `Assets/Editor/FrozenTrainMinimalInteriorPass.cs` — final stripped interior, materials and scene refresh.
- `Assets/Editor/FrozenTrainBuilder.cs` — base geometry **only if needed**.
- `Assets/Editor/FrozenTrainUnifiedAuthoring.cs` — rebuild order if changing passes.

Current contract: 5 cars × **24 × 9.2 m**, 4 inter-car vestibules × **4 × 9.2 m**, three decks. Cars 2–5 have right/+Z corridor, left functional compartment; locomotive main deck private. Only vestibules have wall-mounted vertical ladders (two each), never in-car stairs. Bare, untextured structural/door baseline.

Modify the existing owner, not another layered fix. Keep E-interactions, window visibility and door alignment. If architecture materially changes, update `Docs/FiveCarArchitecture.md` / `Docs/MainStreetArchitecture.md`. Verify changed diff once; don't claim Unity compilation without running it. Rebuild once using `Frozen Train > Rebuild Authored Train`.
