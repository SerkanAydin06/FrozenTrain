# FrozenTrain — Right-Side Corridor Architecture

## Permanent circulation model
FrozenTrain is treated as a horizontal building, not a chain of rooms.

- **Lower deck = Infrastructure.** Power, water, fuel, HVAC, pumps, machinery and maintenance. Normal passenger circulation is not routed here.
- **Main deck = Right-side public corridor.** One straight, continuous two-person corridor runs on the +Z/right side from locomotive to the final car.
- **Upper deck = Residential.** Private cabins and quiet local halls. Upper decks are not an inter-car transit route.

## Main-deck rules
- Public corridor occupies roughly Z=1.90 to Z=4.40, about 2.5 m wide.
- The corridor must remain straight and physically clear through every car.
- Inter-car openings and connector bellows align with the same right-side corridor; they are not centred in the car.
- The entire left side of each car is a closed functional compartment.
- The functional compartment is separated from the public corridor by an **opaque full-height wall**.
- There is **exactly one staff/work-area door per car**, centred on that partition.
- People travelling through the train never walk through generator, workshop, greenhouse, living-service or locomotive work areas.
- Work areas are destinations, not parts of the circulation route.
- No glazing is used to expose the work area from the public corridor unless explicitly redesigned later.

## Five-car intent
1. **Locomotive:** right corridor terminates at the front; driver/control equipment stays inside the closed left operations room.
2. **Generator:** right corridor passes beside one closed generator control/work room.
3. **Living:** right corridor passes beside one closed lounge/galley/service room.
4. **Workshop:** right corridor passes beside one closed workshop used only by staff working there.
5. **Greenhouse:** right corridor passes beside one closed greenhouse production room.

## Upper residential rules
- Upper inter-car movement is not part of normal circulation.
- Each car's residential deck is reached locally from that car's vertical access.
- End bulkheads close the old upper through-route.

## Scalability goal
A future 10–15 car train should let passengers walk locomotive-to-tail through one predictable corridor without entering any functional room. Adding a new car means extending the same +Z corridor and adding one closed room on its left.

## Authoring
`Frozen Train > Rebuild Authored Train` applies the normal builder/art passes and then:

1. `FrozenTrainMainStreetArchitecture` — now implements the right-side corridor model.
2. `FrozenTrainMainStreetConnector` — offsets inter-car openings and the connector to the corridor.
3. `FrozenTrainMainStreetSafety` — guarantees the corridor stays clear and partitions keep colliders.

The passes remain idempotent and preserve existing survival interaction bindings.
