---
paths:
  - "Assets/Editor/FrozenTrain*.cs"
  - "Docs/FiveCarArchitecture.md"
  - "Docs/MainStreetArchitecture.md"
---
# Train authoring — current

- Fixed architecture unless explicitly changed: **5 separate coupled 24 × 9.2 m cars**, 3 decks, car spacing 25 m (0.8 m gap), one main-deck gangway per joint; front -X. Lower/upper decks are closed at car ends.
- `FrozenTrainLayout.cs` owns dimensions; `FrozenTrainBuilder.cs` creates base; `FrozenTrainUnifiedAuthoring.cs` sequences passes. Final authoritative geometry cleanup/URP material slots + scene-instance refresh: `FrozenTrainMinimalInteriorPass.cs`. Do not make competing generators or add user-facing rebuild menu entries.
- Locomotive main deck private. Cars 2–5: unobstructed **+Z corridor** (Z 1.90–4.40), left closed functional room with 1 corridor-facing door.
- Vertical access: in-car `CarAccess` (Builder). Each car: one wall-mounted E ladder to the upper deck at the rear (+X) end of the +Z corridor, beside the rear end door, guarded hatch, upper landing off the hall. Locomotive only: technical-deck ladder. No stair rooms/flights, no inter-car lower/upper passages.
- Active visual mode **bare/untextured**: keep structural shell, windows, aligned functional doors, ladders, minimum lighting/exterior. Do not restore retired furniture, snow ornaments, hazard stripes or patterned textures.
- Door panels match real opening geometry; no clipping outside walls. Preserve stable `SlidingDoor`, `DeckLadder`, `E` targets and persistent URP materials. Cabin doors must close.
- Change source authoring, not generated prefab/scene YAML by hand. Update architecture docs **only for material architectural changes**. On geometry changes rebuild using `Frozen Train > Rebuild Authored Train` then save; review affected fresh screenshots when available.
