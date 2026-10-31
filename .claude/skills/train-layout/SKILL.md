---
name: train-layout
description: Use for FrozenTrain dimensions, decks, corridors, vestibules, doors, ladders, structural train geometry and placement.
---
# Train layout — focused workflow

Start with **the specific symbol/file** involved; usually 1–3:
- `Assets/Editor/FrozenTrainLayout.cs` — dimensions and deck data.
- `Assets/Editor/FrozenTrainLongVestibuleArchitecture.cs` — per-car room layout (historic name). Doors/ladders/gangway: `FrozenTrainBuilder.cs`.
- `Assets/Editor/FrozenTrainMinimalInteriorPass.cs` — final stripped interior, materials and scene refresh.
- `Assets/Editor/FrozenTrainBuilder.cs` — base geometry **only if needed**.
- `Assets/Editor/FrozenTrainUnifiedAuthoring.cs` — rebuild order if changing passes.

Current contract: **2 active coupled cars (Locomotive + Generator)** × 24 × 9.2 m, 25 m spacing, separate main + technical-deck gangways, three decks. The generator has the right/+Z corridor and left functional compartment; locomotive main deck private. Remaining car specifications are reserved for later expansion. Ladders are in-car (`CarAccess` in `FrozenTrainBuilder.cs`): one upper ladder per car beside the rear end door; locomotive alone has the technical ladder, from which all lower decks are connected. All upper cabin doors slide +X; hatches have no obstructing rail cage. Bare, untextured structural/door baseline. Full spec: `Docs/FiveCarArchitecture.md`.

Modify the existing owner, not another layered fix. Keep E-interactions, window visibility and door alignment. If architecture materially changes, update `Docs/FiveCarArchitecture.md` / `Docs/MainStreetArchitecture.md`. Verify changed diff once; don't claim Unity compilation without running it. Rebuild once using `Frozen Train > Rebuild Authored Train`.
