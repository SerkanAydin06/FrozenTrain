# FT_Train_Production_V2

## Goal
Raise all five FrozenTrain cars from greybox/prototype presentation to a coherent semi-realistic production-art pass while preserving the locked five-car survival layout and all gameplay bindings.

## Scope
- 01 Locomotive: driver seats, main console, displays, equipment cabinets, traction technical details, cab/exterior dressing
- 02 Generator: generator engine, alternator, fuel tanks, monitoring rack, manifold, technical service details
- 03 Living: facing train seating, dining tables, galley, medical cabinet, heater, luggage shelving
- 04 Workshop: production workbenches, drawers, pegboard tools, vice, machine station, compressor/hydraulic details
- 05 Greenhouse: hydroponic beds, plants, grow lights, irrigation, water/nutrient tanks, pump and climate equipment
- All cars: shared PBR-style generated materials, panel/rib language, technical deck details, upper sleeping accommodation details, exterior body seams/belts, visible wheels/couplers/roof vents, improved fill lighting
- Access: upper/technical hatch visibility and ladder grab rails; runtime ladder landing safety is handled by `DeckLadder.cs`

## Style
Semi-realistic industrial survival train. Functional, maintained under scarcity, cold exterior and slightly warmer occupied areas. No luxury sci-fi neon and no unrelated decorative clutter.

## Free-only production rule
This package is generated deterministically by repository editor code. It does not require Meshy, Tripo, paid asset packs, paid Blender plugins, or manual modelling by the project owner.

## Architecture contract
`FrozenTrainLayout.cs` and `FrozenTrainBuilder.cs` remain the only source of train architecture/circulation. `FrozenTrainProductionArtPro.cs` is a visual skin/detail pass and must not add cars, move deck boundaries, close corridors, or change survival bindings.

## Review
After `Frozen Train > Rebuild Authored Train`, enter Play once so the automatic screenshot set is refreshed. Review all 5 cars x 3 decks plus `tren_dis_on.jpg` and `tren_dis_yan.jpg` through the screenshot artifact workflow.
