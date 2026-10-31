# FrozenTrain — Right-Side Corridor + Inter-Car Vestibule Architecture

## Permanent circulation model
FrozenTrain is treated as a horizontal building carried by a train, not a chain of rooms.

- **5 usable cars, each 24.0 m × 9.2 m.**
- **4 separate inter-car vestibules, each 4.0 m × 9.2 m.**
- **Lower deck = technical infrastructure.**
- **Main deck = public transit + work/life rooms.**
- **Upper deck = residential accommodation.**
- **All normal vertical circulation is outside the cars, inside the inter-car vestibules.**

## Main deck

### Locomotive
- The whole 24 m main deck is a private operations cab.
- No public corridor runs through it.
- The first vestibule enters a private rear landing.
- Console, seats, cabinets and management terminal may use the full width.

### Cars 2–5
- One straight public corridor runs only on the **+Z/right side**.
- Target envelope: **Z=1.90..4.40**, about **2.5 m wide**.
- It must stay clear for the full 24 m length.
- The remaining left side is one opaque functional room.
- Each functional room has exactly one centred staff/work-room door.
- Passengers never walk through generator, living, workshop or greenhouse work areas just to continue along the train.

## Inter-car vestibules

The old narrow connector is replaced by a full 4 m transition module between every pair of cars.

Each vestibule contains:
- **right side:** straight main transit passage aligned with the car corridors;
- **left side:** closed vertical circulation core;
- main-to-upper real stepped stair flight;
- lower-to-main real stepped stair flight;
- a stair-core door from the main transit passage;
- two main-deck end doors, one to each adjacent car;
- two upper residential landing doors, one to each adjacent car;
- two lower technical landing doors, one to each adjacent car;
- guard rails, hazard bands, dedicated lighting, full roof and underframe continuity.

The vestibule is a circulation/service zone. It must not be treated as room space belonging to either adjacent car.

## No in-car stairs

The following are prohibited inside all five 24 m cars:
- normal residential wall ladders;
- residential stair towers;
- technical floor ladders used as the normal deck connection;
- open ladder holes consuming room floor area;
- stair lobbies cut out of generator/living/workshop/greenhouse space.

Legacy `VerticalAccess`, residential stair markers and hatch geometry are removed by the final architecture pass.

## Upper residential deck

- The upper deck uses the full 24 m car length for cabins and a local central hall.
- Five cabin bays per side use the longer body.
- Vestibule upper landings connect to the adjacent residential halls through dedicated doors.
- Nobody exits a stair directly into a private bed/cabin.
- Main-deck right corridor remains the primary long-distance route; upper movement is residential/local.

## Lower technical deck

- Technical systems use the full car footprint.
- There are no main-floor technical ladder holes inside Generator or Workshop anymore.
- Lower-deck access comes from the vestibule stair core and central lower landing doors.

## Functional-room layout principles

- Keep the centred work-room entry clear for at least ~3 m inside the room.
- Put large machinery and storage against outer walls or toward the long-car ends.
- Keep interaction-bound objects reachable without crossing furniture.
- Living seating stays grouped away from the centred staff door; galley/medical occupy the rear service zone.
- Workshop benches run along the outer wall; machine station and tool storage stay out of the entry lane.
- Greenhouse beds form long production rows while preserving a central entry/service gap.
- Locomotive remains full-width private space.

## Scalability

A future 10–15 car train can repeat the same pattern:

`24 m car → 4 m vestibule → 24 m car → 4 m vestibule ...`

Passengers keep one predictable right-side route while staff rooms and vertical circulation remain isolated.

## Authoring

`Frozen Train > Rebuild Authored Train` is the only public authoring command.

Current chain (`FrozenTrainUnifiedAuthoring.cs`):
1. `FrozenTrainBuilder` — canonical five-car prefabs, connector, scene and player.
2. `FrozenTrainProductionArtPro` — FT_PRO materials/textures and shell/light tuning.
3. `FrozenTrainExteriorScenery` — mountains, rocks, snow drifts and rail gauge (scene).
4. `FrozenTrainMainStreetArchitecture` / `Connector` / `Safety` — +Z corridor, shifted main openings, partition colliders.
5. `FrozenTrainLongVestibuleArchitecture` — 4 m vestibules, doors and wall-mounted ladders.
6. `FrozenTrainMinimalInteriorPass` — final authority: strip dressing, baseline materials, cabin doors, minimal exterior, scene refresh.

Removed on 2026-10-31 (their output was always deleted by the minimal pass): ScreenshotQARefinement (car part), ScreenshotQASafety, ExteriorMaterialFix (merged into ExteriorScenery), LongLightingPass, Post24ScreenshotPolish, LongExteriorPass, CirculationFinalizer, UpperDeckCirculation, UpperDeckFinalPolish, FinalScreenshotFix, ProductionArt wrapper.

These passes preserve existing gameplay interaction components when moving their authored roots.