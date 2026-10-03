# FrozenTrain — Five-Car Architecture Lock

The train remains fixed at five cars. No story content and no additional cars are introduced by this architecture pass.

## Global structure

Every car now has three usable vertical zones:

1. **Basement / infrastructure deck** — pipes, cable trays, tanks, batteries, waste/water/fuel infrastructure and maintenance access.
2. **Main deck** — driving, social life, work, production and survival-system interaction.
3. **Upper sleeping deck** — bunks, lockers and quiet sleeping circulation.

The upper and basement decks continue between cars through multi-level connector passages. The living car contains the main vertical circulation between all three levels.

## Global rules

- Exactly five cars; no new cars.
- No story/event content in this pass.
- Technical cars remain sealed and windowless on the main deck.
- Windows exist only where they make functional sense.
- Tall furniture, cabinets, machinery and bunks never block a window opening.
- Main-deck circulation remains clear.
- Upper sleeping decks keep a continuous central corridor.
- Basement decks keep a continuous service corridor.
- Runtime gameplay code does not generate train geometry; the editor authoring command writes the layout into scene/prefab assets.
- `Frozen Train > Rebuild Authored Train` is the primary authoring command going forward.

## 01 — Locomotive

### Main deck
- Forward cab remains windowed.
- Main driving console and operator seats face the direction of travel.
- Rear half is a sealed technical/control zone.
- Tall cabinets and management terminal remain in solid-wall zones.
- Forward/side sight-lines remain unobstructed.

### Upper deck
- Compact crew sleeping berths and lockers.
- Sealed walls for insulation and darkness.

### Basement
- Traction battery bank.
- Drive electronics and cable trunks.
- Clear service corridor.

## 02 — Generator

### Main deck
- Fully sealed machine room with no side windows.
- Generator mass occupies one machinery side.
- Electrical cabinets occupy the opposite wall.
- Central service route remains clear.

### Upper deck
- Insulated sleeping berths above the machinery deck.
- Central sleeping corridor remains clear.

### Basement
- Fuel tanks.
- Generator manifold and utility pipes.
- Cable/service route through the center.

## 03 — Living

### Main deck
- Central social/lounge zone is windowed.
- Seating uses real train-style **four-person bays**: two double seats face each other along the train axis with a table between them.
- Seats do not face the glass directly.
- Galley, medical cabinet and heater sit in solid end zones.
- Window sight-lines remain completely open above the low seating/table level.
- This car contains the stairs to the upper sleeping deck and basement infrastructure deck.

### Upper deck
- Primary sleeping deck with the highest bunk capacity.
- Bunks stay against the sides with a clear central corridor.
- Lockers occupy end/solid zones.

### Basement
- Fresh-water tank.
- Waste-water tank.
- HVAC/service plant.
- Main service corridor remains clear.

## 04 — Workshop

### Main deck
- Fully sealed, no side windows.
- Workbenches follow the walls.
- Machine station and cabinets stay outside the central aisle.
- Open assembly/service route through the middle.

### Upper deck
- Sleeping berths and lockers.
- Clear center corridor.

### Basement
- Compressor.
- Parts storage.
- Tool power bus and cable trays.

## 05 — Greenhouse

### Main deck
- Fully sealed climate-controlled grow room.
- Two hydroponic crop lanes run along the sides.
- Wide central working aisle.
- Water tank, pump and climate controls occupy utility zones.

### Upper deck
- Sleeping berths/lockers separated from the grow room by the structural upper-deck slab.

### Basement
- Nutrient tank.
- Irrigation reserve.
- Climate-service rack.
- Continuous pipe/cable service corridor.

## Exterior and interaction rules

- The locomotive front faces **-X**. Exterior scenery must move toward **+X** to create the correct forward-motion illusion.
- Snow emitters stay outside the train body on both lateral sides; snow must never spawn inside cars.
- Interaction targeting uses a forgiving aim radius and a short target-retention grace period so the `E` prompt does not flicker with tiny camera movements.
