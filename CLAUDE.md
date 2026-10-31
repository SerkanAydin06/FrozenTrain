# FrozenTrain — Claude Code Guide

## Goal
Work on the existing Unity survival-management game efficiently. Prefer focused edits over broad exploration.

## Project facts
- Engine: Unity 6.3 LTS, URP, Windows.
- Main scene: `Assets/Scenes/FPSPrototype.unity`.
- Exactly 5 usable cars: Locomotive, Generator, Living, Workshop, Greenhouse.
- Do not add cars or story/NPC narrative unless explicitly requested.
- **Current authored footprint: 24.0 m x 9.2 m per usable car.**
- **There are 4 separate inter-car vestibules, each 4.0 m x 9.2 m. Car centres are 28.0 m apart.**
- Train has 3 full-size decks:
  - lower: technical infrastructure;
  - middle: locomotive private cab, or right-side public corridor + closed functional room in cars 2–5;
  - upper: sleeping/accommodation using the full car footprint.
- **No normal vertical circulation is inside a usable car.** All main↔upper and lower↔main access uses upright E-interactable ladders in the four inter-car vestibules (eight ladders in total).
- Each vestibule is a single open passage without an enclosed stair room or inner stair partition; two wall-mounted upright ladders along the -Z side serve the decks.
- Locomotive main deck is fully private; its rear vestibule enters a private landing.
- Locomotive front is -X. Exterior scenery moves +X.
- Public authoring workflow: `Frozen Train > Rebuild Authored Train`.
- **PC render fidelity (2026-10-31):** `PC_RPAsset.asset` uses 4x MSAA; the authored player camera uses SMAA High and a 0.10–220 m clip range. `FrozenTrainProductionArtPro.cs` owns the less repetitive floor/wall textures and trilinear/aniso import settings. Keep these values consistent with the serialized materials and four texture `.meta` files when editing.
- **Current rebuild mode: bare interiors (user request 2026-10-31).** The last pass,
  `Assets/Editor/FrozenTrainMinimalInteriorPass.cs`, removes all interior furniture,
  machines, consoles, screens, beds, storage and other dressing. Only structural shell,
  windows, partition walls, plain passage/cabin doors and vestibule vertical ladders remain,
  with invisible basic lights so the scene is navigable.
- Keep this minimal baseline with unadorned doors, wall-mounted vertical ladders and uncluttered exterior (no roof drifts, orange trim, oversized exterior decals) until the user requests individual elements to be reintroduced.
  Prior authored art sources are retained for later work but are stripped from rebuilt prefabs.
  Do not put removed props back automatically in a subsequent visual pass.

## Source of truth
- Train layout data (24 m size, 4 m vestibule spacing, decks, contents, E bindings): `Assets/Editor/FrozenTrainLayout.cs`
- Builder (turns the layout into prefabs + scene, from scratch each run): `Assets/Editor/FrozenTrainBuilder.cs`
- Stable production-art entry point: `Assets/Editor/FrozenTrainProductionArt.cs`
- Five-car production visual/material implementation: `Assets/Editor/FrozenTrainProductionArtPro.cs`
- Base right-side corridor split: `Assets/Editor/FrozenTrainMainStreetArchitecture.cs`
- Base main-opening alignment: `Assets/Editor/FrozenTrainMainStreetConnector.cs`
- **Final long-car/inter-car circulation authority:** `Assets/Editor/FrozenTrainLongVestibuleArchitecture.cs` (two DeckLadder components per connector, compact ladder openings)
- **24 m technical lighting authority:** `Assets/Editor/FrozenTrainLongLightingPass.cs`
- **Minimal exterior authority:** `Assets/Editor/FrozenTrainMinimalInteriorPass.cs`. The older `FrozenTrainLongExteriorPass.cs` is no longer in the rebuild chain; do not reactivate it while this minimalist baseline is in effect.
- Rebuild entry point: `Assets/Editor/FrozenTrainUnifiedAuthoring.cs`
- Architecture contracts: `Docs/FiveCarArchitecture.md`, `Docs/MainStreetArchitecture.md`
- Survival state: `Assets/Scripts/Management/TrainPrototypeState.cs`
- One-slot manual run save/load: `F5` / `F9` or the management panel. Saved in Unity PlayerPrefs key `FrozenTrain.RunSave.V1` (dynamic state and player position, no scene geometry).
- Simulation: `Assets/Scripts/Management/SurvivalSimulation.cs`
- Management UI: `Assets/Scripts/Management/TrainManagementPanel.cs`
- Art production contract: `Docs/ArtPipeline.md`
- Art source/license ledger: `ArtSource/AssetLedger.csv`

Legacy note: `FrozenTrainUpperDeckCirculation.cs`, `FrozenTrainUpperDeckFinalPolish.cs` and `FrozenTrainCirculationFinalizer.cs` may still exist in the repository for history/compatibility, but they are **not part of the current rebuild chain** and must not be used to restore in-car ladders, stair lobbies or the old 18 m circulation model.

## Cost / context discipline
- Do not scan the whole repository before starting.
- Start with `git status`, then search exact symbols/filenames and read only relevant files/ranges.
- Do not read generated Unity folders (`Library`, `Temp`, `Logs`, `obj`, `UserSettings`, builds).
- Do not read `.meta` files unless GUID/import behavior is directly relevant.
- For normal tasks, inspect at most 2–5 source files before editing; expand only when evidence requires it.
- Prefer `git grep`/targeted search over recursive full-file reads.
- Do not use subagents for routine edits, lookups, or verification.
- Do not browse the web unless current external information is necessary.
- Keep responses concise: findings, files changed, verification, exact Unity step if required.
- Do not repeatedly explain project history already recorded here.

## Editing rules
- Preserve the existing 5-car architecture unless the user explicitly changes that constraint.
- Preserve **24 m usable cars + 4 m inter-car vestibules + 28 m car-centre spacing** unless explicitly redesigned.
- Visual train geometry is authored in editor/prefab code, not spawned as gameplay geometry at runtime.
- Keep `Frozen Train` menu to one public command: `Rebuild Authored Train`.
- Reuse existing systems and object names before creating parallel gameplay systems.
- Avoid decorative clutter that blocks corridors, windows, doors, vestibule landings or interactions.
- Keep window sight-lines clear in windowed spaces.
- Preserve `E` interaction stability and existing component bindings when moving/replacing authored objects.
- **Never place vertical access inside a usable car.** Only two straight wall-mounted vertical ladders per vestibule are allowed; do not restore the stair room, partitions, doors, treads, guards, orange bands or decorative markers.
- Cars 2–5: right corridor stays physically clear for the complete 24 m body; all functional content stays behind the opaque partition on the left.
- Locomotive: no public through-corridor; full-width private cab.
- Upper residential halls/cabins must use the full 24 m car area and receive access from adjacent vestibule upper landings.
- Lower technical decks receive access from adjacent vestibule lower landings; do not cut new holes in car floors.

## Art production rules
- The project owner is not expected to model, UV, rig, or texture by hand.
- Do not require paid 3D-generation services, paid asset packs, or paid plugins.
- For 3D/character/material work, follow `Docs/ArtPipeline.md` and use the `free-art-pipeline` skill.
- Prefer: existing asset -> verified free/CC0 asset -> AI-written Blender Python -> MakeHuman/MPFB + Mixamo for humanoids -> retain greybox until a free route exists.
- Record every third-party art/audio/animation source and license in `ArtSource/AssetLedger.csv`.
- Production art should replace primitive visuals without breaking gameplay bindings, the +Z corridor, functional-room door clearances, vestibule stairs/landings, or connector access.
- Exterior/generated materials must remain persistent URP assets; do not create prefab references to temporary in-memory materials that can render magenta later.

## Workflow
1. Read only the source-of-truth file(s) relevant to the request.
2. Make one coherent change instead of many tiny parallel systems.
3. Check `git diff --check` and inspect the changed diff only.
4. If Unity cannot be run locally, say so; do not claim compilation succeeded.
5. If authored geometry changed, tell the user to run `Frozen Train > Rebuild Authored Train` once after pulling.

Use the project skills when their descriptions match the task.

## Visual review loop
- Screenshot capture is ON by default during every Editor Play of `FPSPrototype`. Turn it off via `Tools > FrozenTrain > Capture Screenshots on Play`. The V2 EditorPrefs key intentionally migrates old opt-in OFF states. `Assets/Scripts/DevTools/TrainScreenshotCapture.cs` uses a separate camera and scene-object-driven car/deck, ladder, connector and door views, not static screenshot coordinates. It removes old JPG/PNG snapshots before creating the latest set; do not accumulate archived copies.
- Car views use `vagonN_katK.jpg`, `_ters` as applicable; adaptive extras use `gecisN_*`, `kapi_*` and `tren_dis_*`. Capture positions depend on current visible geometry and collision safety.
- Vestibule QA views are dynamically named `gecisN_lowerfloor`, `gecisN_mainfloor`, `gecisN_upperfloor`, `gecisN_upperladder`, `gecisN_lowerladder` where geometry exists. All images are 1600x900 JPG quality 92 (max 60 per run).
- `Docs/Screenshots/README.md` lists capture time, labels and camera positions.
- Running Editor Play writes screenshots only to local `Docs/Screenshots/`; they become visible to a remote GitHub review **only after committing/pushing that folder**. The GitHub Actions workflow uploads a CI artifact when pushed. Do not claim that local Play automatically syncs captures to GitHub. Git commit history can retain prior image revisions despite latest-only files in the working tree.
- For visual tasks, inspect the newest screenshot artifact when available; otherwise do not claim pixel-level visual review.
- The latest known pushed screenshot set has capture timestamp 2026-10-30 23:07:41 and predates the wall-ladder/minimal-exterior changes. Do not treat it as current until newly generated images are pushed.
- Compare affected views against the previous screenshot set when judging visual changes.
