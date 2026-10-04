---
paths:
  - "Assets/Editor/FrozenTrain*.cs"
  - "Docs/FiveCarArchitecture.md"
  - "Docs/MainStreetArchitecture.md"
---
# Train authoring rules

- `FrozenTrainLayout.cs` remains the canonical dimensions/content source and `FrozenTrainBuilder.cs` remains the canonical generator. Final architecture/art passes may reorganize generated placement but must not duplicate gameplay state or interaction ownership.
- E interactions are declared in the layout with `.Bind(...)` / `.AsTerminal()`. Architecture passes may move the bound root but must not recreate gameplay bindings by object name.
- Keep exactly 5 cars unless the user explicitly changes that constraint.
- Current car footprint is 18.0 m x 9.2 m; all three decks use the same footprint.
- Lower deck: propulsion, generator, batteries, fuel, electric, water/waste, heating/HVAC, hydraulics, pumps and other technical infrastructure.
- **Locomotive main deck is a full private cab. It has no public through-corridor.**
- **Cars 2–5 use asymmetric circulation:** a straight public corridor runs only on the +Z/right side, approximately Z=1.90..4.40 (about 2.5 m wide, two-person passage).
- The remaining left side of cars 2–5 is one opaque, closed functional compartment for that car's purpose.
- Each functional compartment has exactly one centred corridor-facing staff/work-room door. Travelling passengers never cross the work room.
- A separate end-positioned Residential Access door may exist, but it must enter an enclosed stairwell only and must not provide a second entrance to the work room.
- Inter-car main-deck openings from Generator onward align with the same +Z/right corridor. The Generator-to-Locomotive connector enters a private locomotive landing rather than continuing a corridor through the cab.
- The public corridor envelope must stay free of furniture, machines, shelves, QA decoration, hydroponic props and production-art leftovers.
- Upper deck is private/local residential accommodation. It is **not** an inter-car transit route; old upper through-connections stay closed by end bulkheads.
- **Normal residential access uses an enclosed stairwell, not exposed wall ladders or open floor hatches.** Cars 2–5 reach it directly from the public corridor; the locomotive reaches it from inside the private cab.
- Upper stair landings must open to shared residential space, never directly into a private cabin.
- Lower technical access stays restricted to technical ladders unless explicitly changed.
- Keep the locomotive panoramic front glass and correct -X front orientation.
- Keep only one public `Frozen Train` menu command: `Rebuild Authored Train`.
- When circulation geometry changes, update both `Docs/FiveCarArchitecture.md` and `Docs/MainStreetArchitecture.md` in the same change.
- Before adding a new helper, search for an existing builder/helper and extend it when practical.
