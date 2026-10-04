# FrozenTrain — Right-Side Corridor Architecture

## Permanent circulation model
FrozenTrain is treated as a horizontal building, not a chain of rooms.

- **Lower deck = Infrastructure.** Power, water, fuel, HVAC, pumps, machinery and maintenance. Normal passenger circulation is not routed here.
- **Main deck = Right-side public corridor in cars 2–5.** One straight, continuous two-person corridor runs on the +Z/right side from Generator through Greenhouse.
- **Locomotive main deck = Full private cab.** The locomotive is a destination, not a through-route; it has no passenger corridor inside it.
- **Upper deck = Residential.** Private cabins and quiet local halls. Upper decks are not an inter-car transit route.

## Main-deck rules
- Cars **2–5** use a public corridor roughly at Z=1.90 to Z=4.40, about 2.5 m wide.
- That corridor must remain straight, uncluttered and physically clear.
- Inter-car openings from Generator onward align with the same right-side corridor.
- The Generator-to-Locomotive connector enters a private rear landing inside the locomotive; the corridor does not continue through the locomotive.
- The left side of cars 2–5 is one opaque, closed functional compartment.
- The functional compartment is separated from the public corridor by an opaque full-height wall.
- Each functional compartment has exactly one centred corridor-facing **staff/work-room door**.
- A second, clearly separate **Residential Access** door may exist near the car end, but it only enters the enclosed stairwell and never opens into the work room.
- Passengers travelling through the train never cross generator, workshop, greenhouse or living work areas.
- Old centre-aisle/glazed-frontage decoration must not remain in the right-side corridor.

## Locomotive intent
- The whole main deck is one private operations/cab space.
- Driver console, driver seats, equipment cabinets and management terminal can use the full car width.
- The right-side transit corridor ends when entering the locomotive.
- The locomotive has its own enclosed crew/residential stairwell inside the private cab.

## Cars 2–5 intent
1. **Generator:** right public corridor + one closed generator control/work room.
2. **Living:** right public corridor + one closed lounge/galley/medical room.
3. **Workshop:** right public corridor + one closed staff workshop.
4. **Greenhouse:** right public corridor + one closed greenhouse production room.

## Residential access rules
- Exposed wall ladders are not normal residential access.
- Old open ladder hatches are closed and made walkable.
- Each car uses an **enclosed stairwell volume** with a main-deck access door and an upper residential landing door.
- In cars 2–5, the stairwell is separated from the work room and is reached directly from the public corridor.
- In the locomotive, the stairwell is reached from inside the private cab.
- Upper landings never open into private sleeping cabins.
- Technical-deck ladders in Generator and Workshop remain separate restricted maintenance access.

## Scalability goal
A future 10–15 car train should let passengers walk from Generator toward the tail through one predictable corridor without entering any functional room. The locomotive remains a private destination at the head of the train rather than another corridor segment.

## Authoring
`Frozen Train > Rebuild Authored Train` applies the normal builder/art passes and then:

1. `FrozenTrainMainStreetArchitecture` — builds the right-side corridor/work-room model.
2. `FrozenTrainMainStreetConnector` — offsets inter-car openings and connectors to the corridor.
3. `FrozenTrainMainStreetSafety` — applies base physical cleanup.
4. `FrozenTrainCirculationFinalizer` — enforces the full-private locomotive, removes corridor clutter, closes obsolete ladder hatches, removes exposed residential ladders and creates enclosed residential stairwells.

The passes remain idempotent and preserve existing survival interaction bindings.
