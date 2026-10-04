# FrozenTrain — Main Street Architecture

## Permanent circulation model
FrozenTrain is treated as a horizontal building, not a chain of rooms.

- **Lower deck = Infrastructure.** Power, water, fuel, HVAC, pumps, machinery and maintenance. Normal passenger circulation is not routed here.
- **Main deck = Main Street.** This is the continuous public transit spine from locomotive to the last car. Functional rooms sit beside it rather than inside the through-route.
- **Upper deck = Residential.** Private cabins and quiet local halls. Upper decks are not an inter-car transit route.

## Main Street rules
- Target public width is roughly 2.7–2.8 m inside each car.
- The centre route must stay visually and physically clear.
- Generator, workshop and greenhouse work zones are separated from Main Street with framed safety glazing and explicit access openings.
- Living-car social frontage can stay open to Main Street.
- Locomotive Main Street terminates at a controlled cab lobby; the cab is reached through an interactable sliding security door.
- Future cars must continue the same centreline and visual language.
- No gameplay prop may be placed in the Main Street envelope unless it is intentionally a transit feature.

## Upper residential rules
- Upper inter-car movement is not part of normal circulation.
- Each car's residential deck is treated as its own quiet zone reached from that car's vertical access.
- End bulkheads physically close the old upper connector route.
- Local hallways only distribute people from the landing to cabins.

## Scalability goal
A future 10–15 car train should still allow a passenger to move from the locomotive end to the final car without walking through generator machinery, workshop benches, bedrooms, hydroponic beds, or other private/work areas. Functional zones are destinations off the public spine, not obstacles in it.

## Authoring
`Frozen Train > Rebuild Authored Train` runs the existing builder and art/QA passes, then applies:

1. `FrozenTrainMainStreetArchitecture`
2. `FrozenTrainMainStreetSafety`

The Main Street pass must remain idempotent and must preserve existing survival interaction bindings.
