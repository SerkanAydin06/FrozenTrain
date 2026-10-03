---
paths:
  - "Assets/Scripts/**/*.cs"
---
# Gameplay C# rules

- FrozenTrain is survival-management, not a story-driven action game.
- Do not add narrative events, scripted NPC scenarios, new cars, or quest/story systems unless explicitly requested.
- Keep survival state centralized in `TrainPrototypeState.cs`; avoid duplicate state in UI or interactables.
- `SurvivalSimulation.cs` advances simulation; UI should display/control state rather than own simulation rules.
- Physical interactables should call the same state methods used by the management UI.
- Preserve stable `E` targeting; do not replace forgiving targeting with a single fragile raycast.
- Avoid runtime generation of authored train geometry or decorative props.
- Prefer small deterministic methods and existing components over new manager singletons.
- Keep changes compatible with Unity 6.3 LTS / URP and avoid unnecessary heavy effects.