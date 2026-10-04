# FrozenTrain — Three-Deck Five-Car Architecture

The train remains fixed at five cars. No story content is introduced by this architecture.

## Locked footprint

- Every car: **18.0 m long × 9.2 m wide**.
- Every car has **three full-size decks with the same footprint**.
- Car spacing: 18.8 m center-to-center.
- The three decks are functional spaces, not decorative slices.

## Deck 1 — Lower technical deck

The entire lower deck is reserved for technical infrastructure and technical staff.

- Full-length technical service infrastructure runs below the occupied deck.
- Main electrical bus, data cable trays, ventilation, water, return-water and heating lines remain on this level.
- Propulsion motors, traction batteries and inverters are below the locomotive.
- Generator engine, alternator, fuel tanks and manifold are below the generator car.
- Fresh/waste water, heating boiler, HVAC and water treatment are below the living car.
- Compressor, hydraulics, workshop power distribution and parts infrastructure are below the workshop.
- Greenhouse water tank, pumps, nutrients, irrigation and climate plant are below the greenhouse.
- Access from the main deck remains restricted to dedicated technical ladders in the generator and workshop cars.

## Deck 2 — Main occupied deck

This is the normal passenger/staff level, but passenger circulation and work areas are physically separated.

### Permanent circulation geometry
- A **single straight public corridor** runs along the **+Z/right side** of every car.
- Corridor target envelope is approximately **Z=1.90 to Z=4.40**, about **2.5 m wide**, enough for two people to pass.
- The corridor stays straight through every car and continues through offset inter-car connectors aligned to the same +Z position.
- The remaining left side of the car is one **opaque, closed functional compartment**.
- The partition is full height and does not use viewing glass.
- Each car has **one centred corridor-facing staff door** into its functional compartment.
- A passenger moving from the locomotive toward the tail never enters a generator room, workshop, greenhouse, lounge/work room or cab workspace just to continue along the train.

### Functional rooms
- **01 Locomotive:** driving, supervision and train controls stay inside the closed left operations room. The public corridor terminates at the locomotive front.
- **02 Generator:** operator/control room stays inside one closed staff compartment; generator machinery remains below.
- **03 Living:** lounge/dining/galley/medical functions stay inside one closed living compartment; the public corridor passes beside it.
- **04 Workshop:** repair benches, tools and work machines stay inside one closed staff workshop.
- **05 Greenhouse:** crops and human production work stay inside one closed greenhouse compartment; pumps/tanks/climate machinery remain below.
- The locomotive keeps one single-piece panoramic front windshield.

## Deck 3 — Upper residential deck

The upper deck is private/local accommodation, not a long-distance route.

- Upper decks do **not** form an inter-car passenger corridor.
- Each car's upper level is reached from that car's own vertical access and local shared landing.
- Sleeping compartments/beds and lockers occupy the residential zones.
- Old upper inter-car routes stay physically closed with end bulkheads.
- Ladders must never exit directly into a private cabin.

## Windows

- Locomotive: panoramic single-piece front glass plus limited cab side glazing.
- Existing exterior windows may remain, but the internal work-room/corridor separator is opaque.
- Lower technical and upper sleeping exterior walls remain primarily sealed for insulation and protection.

## Environment

- Locomotive front faces -X; exterior scenery moves +X to read as forward travel.
- Snow emitters are kept far outside the train body and have no lateral velocity toward the train.
- The train stands on a track: underframe and bogies sit below the technical deck, and the snow ground is at rail level, below the whole car body.
- Runtime gameplay does not construct train geometry. `Frozen Train > Rebuild Authored Train` regenerates the canonical train and then applies production-art, QA and final right-side-corridor architecture passes.
