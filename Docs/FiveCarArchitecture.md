# FrozenTrain — Five-Car Architecture Lock

The train remains fixed at five cars. This document locks the interior architecture before further survival-system expansion.

## Global rules

- No new cars.
- No story/event content in this pass.
- Technical cars do not need windows.
- Tall furniture, cabinets, machinery and bunks never block a window opening.
- Every car preserves a clear central circulation/service aisle.
- Windowed cars use solid wall zones for tall storage and service equipment.
- Runtime code does not generate train visual geometry; the editor authoring tool materializes the result into prefab assets.

## 01 — Locomotive

**Window strategy:** front windshield + side windows only in the forward cab zone.

**Layout:**
- Main driving console at the front.
- Two operator seats behind the console.
- Rear half is a solid technical zone.
- Tall electrical cabinets and the management terminal live only in the solid rear zone.
- Cab glazing remains unobstructed.

## 02 — Generator

**Window strategy:** fully sealed, no side windows.

**Layout:**
- Main generator occupies one side of the machine room.
- Electrical/battery cabinet bank occupies the opposite wall.
- Generator console sits near the service route.
- Overhead pipes remain close to the machinery side.
- Wide central maintenance aisle stays open.

## 03 — Living

**Window strategy:** windows only in the central lounge section; front/rear sleeping/service zones are solid.

**Layout:**
- Four bunk modules sit in the solid end zones.
- Central lounge uses low benches below the window line.
- Dining table sits in the middle without blocking circulation.
- Galley, medical/storage cabinet and heater live in solid wall sections.
- No tall object crosses the window sight-line.

## 04 — Workshop

**Window strategy:** fully sealed, no side windows.

**Layout:**
- Long workbenches follow the side walls.
- Tool boards and storage remain wall-mounted.
- Machine station sits near the rear service zone.
- Central assembly table remains accessible from both sides.
- Central work/circulation aisle remains open.

## 05 — Greenhouse

**Window strategy:** fully sealed climate-controlled grow room, no side windows.

**Layout:**
- Two hydroponic crop lanes run along the sides.
- Wide central aisle remains clear.
- Grow lights align over crop lanes.
- Water tank, pump and control panel form a rear utility bay.
- Irrigation infrastructure stays above/along crop lanes, not in the walking path.
