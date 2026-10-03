# FrozenTrain — Three-Deck Five-Car Architecture

The train remains fixed at five cars. No story content is introduced by this architecture.

## Locked footprint

- Every car: **18.0 m long × 9.2 m wide**.
- Every car has **three full-size decks with the same footprint**.
- Car spacing: 18.8 m center-to-center.
- The three decks are functional spaces, not decorative slices.

## Deck 1 — Lower technical deck

The entire lower deck is reserved for technical infrastructure and technical staff.

- Full-length technical service corridor continues between all five cars.
- Main electrical bus, data cable trays and ventilation trunks run below the human deck.
- Main water, return-water and heating pipes run below the human deck.
- Propulsion motors, traction batteries and inverters are below the locomotive.
- Generator engine, alternator, fuel tanks and manifold are below the generator car.
- Fresh/waste water, heating boiler, HVAC and water treatment are below the living car.
- Compressor, hydraulics, workshop power distribution and parts infrastructure are below the workshop.
- Greenhouse water tank, pumps, nutrients, irrigation and climate plant are below the greenhouse.
- Access from the main deck remains restricted to dedicated technical ladders in the generator and workshop cars.

## Deck 2 — Main human/work deck

This is the normal occupied deck.

- 01 Locomotive: driving, supervision and train controls. Electrical drive hardware stays below.
- 02 Generator: operator/control room. The actual generator machinery stays below.
- 03 Living: social/dining/galley/medical area with train-style four-person seating bays. The galley counter stays below the window sill; the medical cabinet (medicine control) sits between two windows.
- 04 Workshop: human-operated production, repair benches and work machines.
- 05 Greenhouse: crops and human work area. Pumps, tanks and climate machinery stay below.
- Passenger circulation continues between cars through the normal center connector doors.
- Window sight-lines remain clear wherever windows exist.
- The locomotive keeps one single-piece panoramic front windshield.

## Deck 3 — Upper sleeping deck

The upper deck is a sleeping/accommodation level across all five cars.

- A **continuous central corridor** runs through every upper deck and every inter-car connector.
- A person can walk from car 3 to car 5 entirely on the upper corridor.
- Sleeping compartments/beds and lockers occupy the closed side zones.
- The center corridor stays unobstructed.
- Each car has **two compact wall ladders** next to the connector-door area, one on each side.
- Ladder openings are cut into the upper floor so access is physically readable and space-efficient.

## Windows

- Locomotive: panoramic single-piece front glass plus limited cab side glazing.
- Living car: main-deck social windows only; furniture does not block them.
- Generator, workshop and greenhouse main decks remain sealed/windowless unless a future functional requirement changes that.
- Lower technical and upper sleeping exterior walls are primarily sealed for insulation and protection.

## Environment

- Locomotive front faces -X; exterior scenery moves +X to read as forward travel.
- Snow emitters are kept far outside the train body and have no lateral velocity toward the train.
- The train stands on a track: underframe and bogies sit below the technical deck, and the snow ground is at rail level, below the whole car body.
- Runtime gameplay does not construct train geometry. `Frozen Train > Rebuild Authored Train` regenerates every car prefab, the connector prefab, the train's scene placement and the exterior from `Assets/Editor/FrozenTrainLayout.cs` in one pass.
