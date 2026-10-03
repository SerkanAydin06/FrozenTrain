---
paths:
  - "Assets/Editor/FrozenTrain*.cs"
  - "Docs/FiveCarArchitecture.md"
---
# Train authoring rules

- `FrozenTrainFinalThreeDeckAuthoring.cs` is the final layout source of truth. Do not create a second architecture layer or postprocessor.
- Keep exactly 5 cars unless the user explicitly changes that constraint.
- Current car footprint is 18.0 m x 9.2 m; all three decks use the same footprint.
- Lower deck: propulsion, generator, batteries, fuel, electric, water/waste, heating/HVAC, hydraulics, pumps and other technical infrastructure.
- Middle deck: occupied driving/social/work/production spaces.
- Upper deck: sleeping/accommodation; center corridor must remain continuous between cars.
- Preserve two compact wall ladders near the connector/door zone for upper access.
- Lower technical access stays restricted unless explicitly changed.
- Keep the locomotive panoramic front glass and correct -X front orientation.
- Keep only one public `Frozen Train` menu command: `Rebuild Authored Train`.
- When layout geometry changes, update `Docs/FiveCarArchitecture.md` in the same change.
- Before adding a new helper, search for an existing builder/helper and extend it instead.