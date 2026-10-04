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
- Access from the main deck remains restricted to dedicated technical ladders in the Generator and Workshop cars.

## Deck 2 — Main occupied deck

Passenger circulation and work areas are physically separated.

### Locomotive
- The **entire locomotive main deck is private operations space**.
- There is no public passenger corridor running through the locomotive.
- The Generator-to-Locomotive connection enters a private rear landing, then opens into the full cab.
- Driver console, seats, equipment cabinets and management terminal use the full available width.
- The locomotive keeps one single-piece panoramic front windshield.

### Cars 2–5 public corridor
- Generator, Living, Workshop and Greenhouse use a **single straight public corridor** along the **+Z/right side**.
- Target corridor envelope is approximately **Z=1.90 to Z=4.40**, about **2.5 m wide**, enough for two people to pass.
- The corridor remains straight and clear through cars 2–5 and continues through offset inter-car connectors aligned to the same +Z position.
- No furniture, machine, decorative QA prop, shelf, hydroponic element, console or work-light may remain inside the public corridor envelope.
- Main-deck legacy light housings are kept on the work-room side; the public corridor uses its own dedicated lighting line.

### Closed functional rooms
- The remaining left side of cars 2–5 is one **opaque, closed functional compartment**.
- The partition is full height and has no viewing glass.
- Each functional room has exactly **one centred staff/work-room door**.
- Generator controls, Living lounge/galley/medical functions, Workshop benches/tools and Greenhouse production stay inside their own closed rooms.
- A passenger continuing through the train never enters these rooms.

### Residential access
- A second end-positioned door may appear on the corridor partition, but it is **not another work-room entrance**.
- It only opens into a compact enclosed residential stair lobby.
- The stair lobby is physically separated from the work room.

## Deck 3 — Upper residential deck

The upper deck is private/local accommodation, not a long-distance route.

- Upper decks do **not** form an inter-car passenger corridor.
- Sleeping compartments/beds and lockers occupy the residential zones.
- Old upper inter-car routes stay physically closed with end bulkheads.
- Exposed wall ladders are no longer the normal residential access solution.
- Old open ladder hatches are closed and made walkable.
- The main-deck stair lobby arrives at an **upper side landing nook** beside the local residential hall.
- The stair structure must never form a full-height block across the centre upper hallway.
- Cars 2–5 reach residential access directly from the right-side public corridor without entering the work room.
- The locomotive reaches its residential access from inside the private cab.
- Upper landing doors open to shared residential space, never directly into a private cabin.

## Windows

- Locomotive: panoramic single-piece front glass plus limited cab side glazing.
- Existing exterior windows may remain, but work-room/corridor separators in cars 2–5 are opaque.
- Lower technical and upper residential exterior walls remain primarily sealed for insulation and protection.

## Environment

- Locomotive front faces -X; exterior scenery moves +X to read as forward travel.
- Snow emitters are kept far outside the train body and have no lateral velocity toward the train.
- The train stands on a track: underframe and bogies sit below the technical deck, and the snow ground is at rail level, below the whole car body.
- Generated mountain, rock and snow scenery must use persistent URP/Lit material assets; temporary or legacy materials that render magenta are not allowed.
- Runtime gameplay does not construct train geometry. `Frozen Train > Rebuild Authored Train` regenerates the canonical train and then applies production-art, QA, right-side-corridor, persistent exterior-material and final circulation cleanup passes.
