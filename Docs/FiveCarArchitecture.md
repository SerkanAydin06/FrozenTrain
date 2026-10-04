# FrozenTrain — Three-Deck Five-Car Architecture

The train remains fixed at five functional cars. No story content is introduced by this architecture.

## Locked footprint

- Every usable car: **24.0 m long × 9.2 m wide**.
- Every car has three full-size decks with the same footprint.
- Between each pair of cars is a separate **4.0 m long × 9.2 m wide inter-car vestibule module**.
- Car centres are therefore **28.0 m apart**.
- Total authored five-car train body is about **136 m** from locomotive front to greenhouse rear.
- Vestibules are circulation/service modules; they do not count as usable room area inside a car.

## Core circulation rule

There are no normal passenger/residential stairs inside the 24 m cars.

Each of the four inter-car vestibules contains:
- a straight +Z/right-side through passage aligned with the public corridor;
- an enclosed stair core on the left;
- a real stepped flight from the main deck to the upper residential landing;
- a separate real stepped flight from the lower technical landing to the main deck;
- end doors into both adjacent cars on all relevant decks.

This keeps vertical circulation out of locomotive, generator, living, workshop and greenhouse usable floor area.

## Deck 1 — Lower technical deck

The entire lower deck of each 24 m car is technical infrastructure.

- Full-length power, data, ventilation, water, return-water and heating services run here.
- Locomotive: traction motors, batteries and inverter.
- Generator: engine, alternator, fuel storage and manifold.
- Living: fresh/waste water, boiler, HVAC and treatment.
- Workshop: compressor, hydraulics, power distribution and parts.
- Greenhouse: water, nutrient, irrigation, pumps and climate plant.
- Normal vertical access comes from the adjacent inter-car vestibule technical landing, not from a hole or ladder inside the room.

## Deck 2 — Main occupied deck

### Locomotive
- The **entire 24 m locomotive main deck is private operations space**.
- There is no public corridor running through the locomotive.
- The first vestibule enters a private rear landing; the full-width cab begins immediately after it.
- Driver console, seats, cabinets and management terminal may use the full width.
- The locomotive keeps one single-piece panoramic front windshield.

### Cars 2–5 public corridor
- Generator, Living, Workshop and Greenhouse use one straight public corridor along the **+Z/right side**.
- Corridor envelope: approximately **Z=1.90..4.40**, about **2.5 m wide** for two-person passage.
- It stays clear for the full 24 m car length.
- Inter-car vestibule through passages align with exactly the same +Z corridor.
- Furniture, hydroponics, machines, shelves, QA decoration and work lights may never occupy the public corridor envelope.

### Closed functional rooms
The left side of cars 2–5 is one opaque closed work/life room with exactly one centred corridor-facing staff door.

- **Generator:** operator desk, generator controls, monitoring and staff work area.
- **Living:** lounge/seating, galley, medical storage and heater/service zone.
- **Workshop:** benches, machine station, tools and parts storage.
- **Greenhouse:** hydroponic production rows and climate controls.

A passenger travelling through the train never needs to enter any of those rooms.

## Deck 3 — Upper residential deck

- The full 24 m upper deck is residential accommodation.
- Five cabin bays per side use the longer footprint.
- Cabins open onto a local central residential hall.
- There are no ladder holes or stair shafts consuming cabin space.
- Residential access occurs from the upper landing of either adjacent vestibule.
- Vestibule landings open to shared residential hall space, never directly into a private bed area.
- Upper decks are not the primary long-distance transit route; the main public route remains deck 2.

## Inter-car vestibule design

Each 4 m vestibule is a distinct railway-style transition zone:
- full-width insulated shell;
- right-side main through passage with doors at both car ends;
- left-side stair core behind its own door;
- upper residential landing with doors to both adjacent cars;
- lower technical landing with doors to both adjacent cars;
- guard rails, hazard markings and dedicated lighting;
- roof/underframe continuity so the module reads as part of the train rather than an empty gap.

## Windows and exterior

- Locomotive: panoramic front glass plus limited cab side glazing.
- Living uses a longer six-window rhythm appropriate to the 24 m body.
- Work-room/corridor separators remain opaque.
- Bogies, wheels, roof chamfers, body bands and locomotive nose details are positioned from the 24 m footprint, not legacy 18 m coordinates.
- Exterior generated scenery must use persistent URP/Lit materials; magenta missing-material rendering is not acceptable.

## Authoring

`Frozen Train > Rebuild Authored Train` is the only public authoring command.

The final chain is:
1. canonical builder and production art;
2. screenshot/environment QA passes;
3. right-side corridor and shifted main-deck openings;
4. `FrozenTrainLongVestibuleArchitecture` — final 24 m room layout and 4 m vestibules;
5. `FrozenTrainLongLightingPass` — five-point technical lighting for the longer cars;
6. `FrozenTrainLongExteriorPass` — 24 m exterior details.

Runtime gameplay does not construct train geometry.