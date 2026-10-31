---
paths:
  - "Assets/Editor/FrozenTrain*.cs"
  - "Docs/FiveCarArchitecture.md"
  - "Docs/MainStreetArchitecture.md"
---
# Train authoring — current

- Fixed architecture unless explicitly changed: **2 active coupled 24 × 9.2 m cars** (locomotive + generator), 3 decks, car spacing 25 m (0.8 m gap), two narrow gangways at the joint (main + technical), front -X. Other car specs stay dormant for later expansion. Upper decks are closed at car ends.
- `FrozenTrainLayout.cs` owns dimensions; `FrozenTrainBuilder.cs` creates base; `FrozenTrainUnifiedAuthoring.cs` sequences passes. Final authoritative geometry cleanup/URP material slots + scene-instance refresh: `FrozenTrainMinimalInteriorPass.cs`. Do not make competing generators or add user-facing rebuild menu entries.
- Locomotive main deck private. Generator car: unobstructed **+Z corridor** (Z 1.90–4.40), left closed functional room with 1 corridor-facing door.
- Vertical access: in-car `CarAccess` (Builder). Each car: one wall-mounted E ladder to the upper deck at the rear (+X) end of the +Z corridor, beside the rear end door, unobstructed hatch approach, upper landing off the hall. Locomotive only: technical-deck ladder; lower decks connect through narrow service gangways, upper decks stay isolated. Upper cabin doors slide rearward (+X).
- Active visual mode **bare/untextured**: keep structural shell, windows, aligned functional doors, ladders, minimum lighting/exterior. Do not restore retired furniture, snow ornaments, hazard stripes or patterned textures.
- Door panels match real opening geometry; no clipping outside walls. Preserve stable `SlidingDoor`, `DeckLadder`, `E` targets and persistent URP materials. Cabin doors must close.
- Change source authoring, not generated prefab/scene YAML by hand. Update architecture docs **only for material architectural changes**. On geometry changes rebuild using `Frozen Train > Rebuild Authored Train` then save; review affected fresh screenshots when available.
