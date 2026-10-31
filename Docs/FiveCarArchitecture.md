# FrozenTrain — Minimal two-car prototype (current architecture, 2026-10-31)

## Train
- **2 active cars**, each **24.0 m × 9.2 m**, three decks: Locomotive + Generator. Front = -X. Living, Workshop and Greenhouse are not built or placed until later expansion; source specifications remain available.
- Cars are coupled like a real train: car-centre spacing **25 m**, a **0.6 m** visible gap between end-wall outer faces.
- Each joint has **two narrow covered gangways**, one on the main deck and one on the lower technical deck; both keep the coupled car bodies separate. The upper decks remain closed between cars. The lower side skirts are recessed at joints so the separation is visible across the entire height.

## Texture grid standard (2026-10-31)
- **1 texture repeat = 1.00 m** on all floors and interior walls (square power-of-two textures: 1024² or 2048²; 96²/100² are only previews and not mip-friendly).
- Interior clear size is identical for every car and every deck: **24 m × 9 m × 3 m** (whole tiles). Shell walls 0.20 m and floor slabs 0.20 m sit outside the clear grid; car shell 24.2 × 9.2 m, deck pitch 3.2 m (floor tops −3.2 / 0 / 3.2, ceiling 6.2).
- Openings start/end on whole metres from the inner corner and the deck floor top: main-deck windows 2 m wide at X ±1..3, ±5..7, ±9..11, 1 m → 2 m high; end doors 1 m × 2 m at Z 1.5..2.5; cabin doors 1 m × 2 m; hatches 1 × 1 m; corridor Z 1.5..4.5; upper hall Z −1.5..1.5 with six 4 m cabin bays per side (rear +Z bay = stair landing).
- Windows only on the main deck; lower (technical) and upper (cabin) decks have closed side walls.
- `TrainFloorTiling` / `TrainWallTiling` map UVs from the car's inner corner and each deck floor top (values written by `FrozenTrainMinimalInteriorPass` from `FrozenTrainLayout`). Exterior body tiles are sized per axis so length, width and body height hold whole tiles.
- `FrozenTrainLayout.ValidateTextureGrid()` aborts the rebuild if any dimension or opening leaves the grid.

## Decks
- **Lower = technical.** Vertical entry is restricted to the locomotive's dedicated technical ladder; once inside, service personnel can traverse both active lower decks through the single narrow inter-car gangway with sliding rear doors.
- **Main = shared work / living.** Generator car: straight **+Z public corridor** (Z 1.5–4.5) and a closed functional room on the -Z side with one corridor door. Locomotive main deck is the private cab.
- **Upper = private cabins.** Each car's upper deck is reached only by its own ladder; no inter-car passage on this deck.

## Car ends (main + technical decks)
- Both levels have 1 m × 2 m openings at Z 1.5..2.5 (`MainEndOpening`, `LowerEndOpening`, `EndDoorCenterZ`); the locomotive front and final car rear remain closed.
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
- Locomotive and generator now use the same **six-window mirrored X rhythm** on both exterior middle-deck side walls (centres ±10, ±6, ±2 m; width 2 m; 1–2 m high). This fixes the locomotive's former front-only 3-window arrangement and the generator's left solid/right windowed mismatch. The locomotive's forward panoramic windscreen remains.
- Upper cabins preserve mirrored X bay/divider/door spacing (5 bays per side). The +Z rear upper ladder landing **necessarily removes part of one rear cabin wall**; do not mirror a useless closed compartment on the opposite side.
- Lower technical floors remain full-width and symmetric. The locomotive's lower access hatch is placed at -Z, while the upper hatch is at +Z; this is the intentional functional exception. Generator has no technical ladder.
- `ValidateSymmetricBase()` in `FrozenTrainLayout.cs` checks cabin bay positions, room doors, partitions and bilateral windows **before** authoring overwrites prefab/scene files. Gameplay device positions are allowed to differ by function.
- Unity rebuilt prefab renders and walkthrough are still unverified in this environment.
