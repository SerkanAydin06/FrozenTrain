---
paths:
  - "Assets/Editor/FrozenTrain*.cs"
  - "Docs/FiveCarArchitecture.md"
  - "Docs/MainStreetArchitecture.md"
---
# Train authoring rules

- `FrozenTrainLayout.cs` remains the canonical dimensions/content source and `FrozenTrainBuilder.cs` remains the canonical generator. Final architecture/art passes may only reorganize generated visual/layout placement without duplicating gameplay state or interaction ownership.
- E interactions are declared in the layout with `.Bind(...)` / `.AsTerminal()`. Architecture passes may move the bound root but must not recreate gameplay bindings by object name.
- Keep exactly 5 cars unless the user explicitly changes that constraint.
- Current car footprint is 18.0 m x 9.2 m; all three decks use the same footprint.
- Lower deck: propulsion, generator, batteries, fuel, electric, water/waste, heating/HVAC, hydraulics, pumps and other technical infrastructure.
- **Middle deck circulation is permanently asymmetric:** a straight public corridor runs only on the +Z/right side of every car, approximately Z=1.90..4.40 (about 2.5 m wide, two-person passage).
- The remaining left side of each main deck is one opaque, closed functional compartment for that car's purpose.
- Each functional compartment has exactly one corridor-facing staff door. Travelling passengers never cross the work room.
- Inter-car main-deck openings and connector bellows must align with the same +Z/right corridor, not the car centreline.
- Upper deck is private/local residential accommodation. It is **not** an inter-car transit route; old upper through-connections stay closed by end bulkheads.
- Preserve compact vertical access for upper residential areas; ladders must arrive at shared local landings, never inside a private cabin.
- Lower technical access stays restricted unless explicitly changed.
- Keep the locomotive panoramic front glass and correct -X front orientation.
- Keep only one public `Frozen Train` menu command: `Rebuild Authored Train`.
- When circulation geometry changes, update both `Docs/FiveCarArchitecture.md` and `Docs/MainStreetArchitecture.md` in the same change.
- Before adding a new helper, search for an existing builder/helper and extend it instead.
