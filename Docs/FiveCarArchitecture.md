# FrozenTrain — Five-Car Architecture Lock

The train remains fixed at five cars. No story content and no additional cars are introduced by this architecture pass.

## Global structure

The train uses three vertical functional zones, but they are **not all public circulation decks**:

1. **Basement / infrastructure deck** — restricted technical service space for pipes, cable trays, tanks, batteries, HVAC, fuel/water and maintenance infrastructure.
2. **Main deck** — normal driving, social life, work, production and survival interaction.
3. **Upper deck** — sleeping/private accommodation belonging to that specific car.

## Access rules

- Upper decks **do not connect from car to car**.
- Every upper deck is reached from its own main deck using a compact **wall-mounted ladder** and a real floor opening.
- Upper-deck front/rear ends are sealed.
- Basement access is deliberately limited: the main public deck only has technical access hatches/ladders in **02 Generator** and **04 Workshop**.
- Once technical staff enter the basement, the basement service corridor can continue between cars through the lower connector route.
- Living/social passengers do not have a basement opening in the living car.
- All vertical access uses wall ladders to preserve usable floor area; the previous stair-flight layout is obsolete.

## Global rules

- Exactly five cars; no new cars.
- No story/event content in this pass.
- Technical cars remain sealed and windowless on the main deck.
- Windows exist only where they make functional sense.
- Tall furniture, cabinets and machinery never block a window opening.
- Main-deck circulation remains clear.
- Runtime gameplay code does not generate the authored train layout.
- `Frozen Train > Rebuild Authored Train` remains the single public authoring workflow.

## 01 — Locomotive

### Main deck
- The front is one **single-piece panoramic windshield** with no central mullions.
- Driving console and operator seats face the direction of travel.
- Rear half is a sealed technical/control zone.
- Tall cabinets and management terminal remain in solid-wall zones.
- Cab sight-lines stay unobstructed.

### Upper deck
- Private crew sleeping area for this car only.
- Reached by the car's own wall ladder.
- No upper-deck connection to neighbouring cars.

### Basement
- Traction battery bank and drive electronics.
- No public main-deck basement hatch in this car.
- Accessible through the technical basement route after entering from an authorised access car.

## 02 — Generator

### Main deck
- Fully sealed machine room with no side windows.
- Generator mass on the machinery side, electrical cabinets opposite.
- Central service route remains clear.

### Upper deck
- Crew sleeping/private space for this car.
- Own wall ladder; no inter-car upper passage.

### Basement
- **Primary technical basement access point.**
- Wall ladder/hatch from the main generator deck.
- Fuel tanks, generator manifold, pipes and cable service route.

## 03 — Living

### Main deck
- Main social/living level.
- Central lounge remains windowed.
- Seating uses real train-style four-person bays: paired double seats face each other along the train axis with a table between them.
- Seats do not face directly into the glass.
- Galley, medical storage and heater remain in solid end zones.
- Window sight-lines remain clear.
- **No basement hatch** in the passenger living area.

### Upper deck
- This is the residents' private accommodation level.
- Four compact private sleeping rooms are arranged around a central corridor.
- Each room has a bed, locker and shelf.
- Wall ladder lands in a dedicated clear landing area.
- No upper-deck connection to another car.

### Basement
- Fresh/waste water and HVAC infrastructure may exist below, but passengers do not enter from this car.
- Technical staff reach it from the restricted basement route.

## 04 — Workshop

### Main deck
- Fully sealed workshop with wall benches, machine station and clear central work route.

### Upper deck
- Crew sleeping/private area reached by the car's own wall ladder.
- No upper inter-car passage.

### Basement
- **Second technical basement access point.**
- Wall ladder/hatch from the workshop.
- Compressor, parts storage, tool power bus and cable trays.

## 05 — Greenhouse

### Main deck
- Fully sealed climate-controlled grow room.
- Two hydroponic crop lanes with a clear central working aisle.

### Upper deck
- Sleeping/private space for this car, reached only by its own wall ladder.
- No upper inter-car passage.

### Basement
- Nutrient tank, irrigation reserve and climate-service equipment.
- No public hatch from the greenhouse main deck; technical route access only.

## Exterior and interaction rules

- The locomotive front faces **-X**. Exterior scenery moves toward **+X** to create the correct forward-motion illusion.
- Snow emitters are rebuilt as narrow **far-exterior strips at Z +/-18.5m** with zero lateral drift toward the train.
- Any old snow particle systems in the playable scene are removed before the exterior emitters are created.
- Snow must never spawn or travel through the interior volume.
- Interaction targeting keeps its forgiving aim radius and short target-retention grace period so the `E` prompt remains stable with small camera movement.
