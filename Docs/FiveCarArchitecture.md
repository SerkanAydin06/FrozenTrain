# FrozenTrain — Five Coupled Cars (current architecture, 2026-10-31)

## Train
- **5 separate cars**, each **24.0 m × 9.2 m**, three decks: Locomotive, Generator, Living, Workshop, Greenhouse. Front = -X.
- Cars are coupled like a real train: car-centre spacing **25 m**, a **0.8 m** visible gap between end-wall outer faces.
- Each joint has one short covered **gangway** on the main deck only (floor, rubber bellows, roof, coupler). Lower and upper decks are closed at every car end, so the cars read as separate bodies.

## Decks
- **Lower = technical.** Only reachable from the locomotive (technical ladder). No inter-car passage on this deck.
- **Main = shared work / living.** Cars 2–5: straight **+Z public corridor** (Z 1.90–4.40) and a closed functional room on the -Z side with one corridor door. Locomotive main deck is the private cab.
- **Upper = private cabins.** Each car's upper deck is reached only by its own ladder; no inter-car passage on this deck.

## Car ends (main deck)
- End openings are 1.30 m wide, centred at Z 2.55 in the corridor (`FrozenTrainLayout.MainEndOpening`, `EndDoorCenterZ`).
- The **rear (+X) end of every car except the last** has the sliding door to the next car (`CarAccess/RearEndDoor`). The front opening of the next car has no door, so each joint has exactly one door.

## Vertical access (in-car, `CarAccess`, authored by `FrozenTrainBuilder`)
- **Every car:** `UpperDeckLadder` — compact wall-mounted vertical ladder on the +Z outer wall at the rear end of the corridor, beside the rear door. Hatch in the upper floor with guard rails on three sides.
- Upper deck: the rear +Z cabin is shortened (`UpperLandingX`) so the hatch opens onto a small landing off the residential hall.
- **Locomotive only:** `TechnicalDeckLadder` on the -Z wall at the rear of the cab, hatch in the main floor with guard rails.
- All ladders use `DeckLadder` (E to climb / descend); guard rails forward the same interaction.

## Rebuild chain
`Frozen Train > Rebuild Authored Train` (`FrozenTrainUnifiedAuthoring.cs`):
1. `FrozenTrainBuilder` — cars, end openings, floor hatches, upper landing, `CarAccess`, gangway connector, scene, player.
2. `FrozenTrainProductionArtPro` — FT_PRO materials/textures.
3. `FrozenTrainExteriorScenery` — mountains, rocks, snow drifts, rail gauge.
4. `FrozenTrainMainStreetArchitecture` / `Safety` — +Z corridor partition and colliders.
5. `FrozenTrainLongVestibuleArchitecture` (historic name) — per-car room layout.
6. `FrozenTrainMinimalInteriorPass` — final authority: strip dressing, baseline untextured slots, cabin doors, minimal exterior, scene refresh.
