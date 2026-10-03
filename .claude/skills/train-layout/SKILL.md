---
name: train-layout
description: Focused workflow for changing FrozenTrain car dimensions, three-deck architecture, corridors, ladders, windows, connectors, technical deck equipment, or authored interior layout.
---
# Train layout

For train architecture/layout work, start with only these files:

1. `Assets/Editor/FrozenTrainLayout.cs` (data: dimensions, decks, per-car props, E bindings)
2. `Assets/Editor/FrozenTrainBuilder.cs` (shell, floors, ladders, connectors, scene)
3. `Docs/FiveCarArchitecture.md`

Read another file only if one of these directly references behavior that must change.

## Locked architecture
- 5 cars only.
- 18.0 m x 9.2 m footprint per car unless the user explicitly requests a new size.
- 3 full-size decks.
- Lower deck = technical plant/infrastructure.
- Middle deck = occupied/social/work/production.
- Upper deck = sleeping/accommodation with continuous center corridor between cars.
- Two wall ladders near connector/door areas for upper access.
- Locomotive front = one-piece panoramic glass.

## Implementation rules
- Change the layout data first; touch the builder only for new kinds of geometry. Do not add another revision/postprocessor layer.
- Keep corridors, connector openings, windows and ladder landings unobstructed.
- Move technical machinery below rather than duplicating it on middle deck.
- Preserve object names used by physical interactions where possible.
- Update `Docs/FiveCarArchitecture.md` with architectural changes.
- After layout changes, the user must run `Frozen Train > Rebuild Authored Train` once.

Verify with `git diff --check` and inspect only the layout-related diff.