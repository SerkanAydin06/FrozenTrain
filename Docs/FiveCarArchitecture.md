# FrozenTrain — Minimal two-car prototype (current architecture, 2026-10-31)

## Train
- **2 active cars**, each **24.0 m × 9.2 m**, three decks: Locomotive + Generator. Front = -X. Living, Workshop and Greenhouse are not built or placed until later expansion; source specifications remain available.
- Cars are coupled like a real train: car-centre spacing **25 m**, a **0.8 m** visible gap between end-wall outer faces.
- Each joint has **two narrow covered gangways**, one on the main deck and one on the lower technical deck; both keep the coupled car bodies separate. The upper decks remain closed between cars. The lower side skirts are recessed at joints so the separation is visible across the entire height.

## Decks
- **Lower = technical.** Vertical entry is restricted to the locomotive's dedicated technical ladder; once inside, service personnel can traverse both active lower decks through the single narrow inter-car gangway with sliding rear doors.
- **Main = shared work / living.** Generator car: straight **+Z public corridor** (Z 1.90–4.40) and a closed functional room on the -Z side with one corridor door. Locomotive main deck is the private cab.
- **Upper = private cabins.** Each car's upper deck is reached only by its own ladder; no inter-car passage on this deck.

## Car ends (main + technical decks)
- Both levels have 1.30 m wide openings, centred at Z 2.55 (`MainEndOpening`, `LowerEndOpening`, `EndDoorCenterZ`); the locomotive front and final car rear remain closed.
- Each intermediate **rear (+X)** end has one sliding door for each level (`RearEndDoor` and `LowerRearEndDoor`). The next car's front opening is open into its gangway; each level has exactly one door per joint.

## Vertical access (in-car, `CarAccess`, authored by `FrozenTrainBuilder`)
- **Each active car:** `UpperDeckLadder` — compact wall-mounted vertical ladder on the +Z outer wall at the rear end of the corridor, beside the rear door. Its approach is open; the former obstructive hatch-guard rail cage was removed.
- Upper deck: the rear +Z cabin is shortened (`UpperLandingX`) so the hatch opens onto a small landing off the residential hall.
- **Locomotive only:** `TechnicalDeckLadder` on the -Z wall at the rear of the cab, with an unobstructed hatch approach.
- All ladders use `DeckLadder` (E to climb / descend). Confirm safe landing and fall prevention in Unity Play Mode.

## Rebuild chain
`Frozen Train > Rebuild Authored Train` (`FrozenTrainUnifiedAuthoring.cs`):
1. `FrozenTrainBuilder` — 2 active cars, end openings, floor hatches, upper landing, `CarAccess`, gangway connector, scene, player.
2. `FrozenTrainProductionArtPro` — FT_PRO materials/textures.
3. `FrozenTrainExteriorScenery` — mountains, rocks, snow drifts, rail gauge.
4. `FrozenTrainMainStreetArchitecture` / `Safety` — +Z corridor partition and colliders.
5. `FrozenTrainLongVestibuleArchitecture` (historic name) — per-car room layout.
6. `FrozenTrainMinimalInteriorPass` — final authority: strip dressing, baseline untextured slots, cabin doors, minimal exterior, scene refresh.

## Upper cabin doors and QA
- Every upper cabin sliding leaf moves toward the **train rear (+X)**, with small inward corridor clearance to avoid the rear landing bulkhead.
- `TrainScreenshotCapture` samples lower service gangways (`teknik_gecis_*`), lower exterior coupling seams (`alt_dis_ayrim_*`), ladders and more widely framed doors. Only the latest generated images are kept locally and must be pushed for remote review.

## Structural symmetry audit (2026-10-31)
- Locomotive and generator now use the same **six-window mirrored X rhythm** on both exterior middle-deck side walls (±9.2, ±5.5, ±1.8 m; width 2.2 m). This fixes the locomotive's former front-only 3-window arrangement and the generator's left solid/right windowed mismatch. The locomotive's forward panoramic windscreen remains.
- Upper cabins preserve mirrored X bay/divider/door spacing (5 bays per side). The +Z rear upper ladder landing **necessarily removes part of one rear cabin wall**; do not mirror a useless closed compartment on the opposite side.
- Lower technical floors remain full-width and symmetric. The locomotive's lower access hatch is placed at -Z, while the upper hatch is at +Z; this is the intentional functional exception. Generator has no technical ladder.
- `ValidateSymmetricBase()` in `FrozenTrainLayout.cs` checks cabin bay positions, room doors, partitions and bilateral windows **before** authoring overwrites prefab/scene files. Gameplay device positions are allowed to differ by function.
- Unity rebuilt prefab renders and walkthrough are still unverified in this environment.
