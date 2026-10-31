# FrozenTrain — Five Coupled Cars (current architecture, 2026-10-31)

## Train
- **5 separate cars**, each **24.0 m × 9.2 m**, three decks: Locomotive, Generator, Living, Workshop, Greenhouse. Front = -X.
- Cars are coupled like a real train: car-centre spacing **25 m**, a **0.8 m** visible gap between end-wall outer faces.
- Each joint has **two narrow covered gangways**, one on the main deck and one on the lower technical deck; both keep the coupled car bodies separate. The upper decks remain closed between cars. The lower side skirts are recessed at joints so the separation is visible across the entire height.

## Decks
- **Lower = technical.** Vertical entry is restricted to the locomotive's dedicated technical ladder; once inside, service personnel can traverse all five lower decks through narrow inter-car gangways with sliding rear doors.
- **Main = shared work / living.** Cars 2–5: straight **+Z public corridor** (Z 1.90–4.40) and a closed functional room on the -Z side with one corridor door. Locomotive main deck is the private cab.
- **Upper = private cabins.** Each car's upper deck is reached only by its own ladder; no inter-car passage on this deck.

## Car ends (main + technical decks)
- Both levels have 1.30 m wide openings, centred at Z 2.55 (`MainEndOpening`, `LowerEndOpening`, `EndDoorCenterZ`); the locomotive front and final car rear remain closed.
- Each intermediate **rear (+X)** end has one sliding door for each level (`RearEndDoor` and `LowerRearEndDoor`). The next car's front opening is open into its gangway; each level has exactly one door per joint.

## Vertical access (in-car, `CarAccess`, authored by `FrozenTrainBuilder`)
- **Every car:** `UpperDeckLadder` — compact wall-mounted vertical ladder on the +Z outer wall at the rear end of the corridor, beside the rear door. Its approach is open; the former obstructive hatch-guard rail cage was removed.
- Upper deck: the rear +Z cabin is shortened (`UpperLandingX`) so the hatch opens onto a small landing off the residential hall.
- **Locomotive only:** `TechnicalDeckLadder` on the -Z wall at the rear of the cab, with an unobstructed hatch approach.
- All ladders use `DeckLadder` (E to climb / descend). Confirm safe landing and fall prevention in Unity Play Mode.

## Rebuild chain
`Frozen Train > Rebuild Authored Train` (`FrozenTrainUnifiedAuthoring.cs`):
1. `FrozenTrainBuilder` — cars, end openings, floor hatches, upper landing, `CarAccess`, gangway connector, scene, player.
2. `FrozenTrainProductionArtPro` — FT_PRO materials/textures.
3. `FrozenTrainExteriorScenery` — mountains, rocks, snow drifts, rail gauge.
4. `FrozenTrainMainStreetArchitecture` / `Safety` — +Z corridor partition and colliders.
5. `FrozenTrainLongVestibuleArchitecture` (historic name) — per-car room layout.
6. `FrozenTrainMinimalInteriorPass` — final authority: strip dressing, baseline untextured slots, cabin doors, minimal exterior, scene refresh.

## Upper cabin doors and QA
- Every upper cabin sliding leaf moves toward the **train rear (+X)**, with small inward corridor clearance to avoid the rear landing bulkhead.
- `TrainScreenshotCapture` samples lower service gangways (`teknik_gecis_*`), lower exterior coupling seams (`alt_dis_ayrim_*`), ladders and more widely framed doors. Only the latest generated images are kept locally and must be pushed for remote review.
