# FrozenTrain — ChatGPT / Codex repository instructions

Use **concise Turkish**. Prefer direct, narrowly scoped GitHub edits and link the commit. Do not ask the user to make edits when repository write access exists. Do not claim Unity compile/Play testing without running it.

## Fixed current state
- Unity **6.3 LTS**, URP, Windows; `Assets/Scenes/FPSPrototype.unity`.
- **Current prototype = locomotive + generator only (2 coupled cars)**, each 24 × 9.2 m with 3 decks; 25 m spacing, 0.8 m coupling gap and main + technical gangways. Front = -X. More car specifications are dormant in FrozenTrainLayout.cs for later expansion.
- Locomotive main deck private; active generator car has a public +Z corridor with a left closed workroom. Ladders are **inside cars** at the rear end of the corridor beside the rear end door (1 upper ladder per car); only the locomotive provides vertical access to the technical deck, which now connects across all cars. Upper cabin doors slide +X (train rear); ladder hatch approaches are open. See `Docs/FiveCarArchitecture.md`.
- **Bare, untextured baseline** until requested otherwise. No furnishing, orange accents, patterned material restoration or bulky exterior decoration. 5 persistent PBR-ready URP slots in `Assets/Art/Materials/TrainBaseline/`: `01_Body`, `02_Walls`, `03_Floors`, `04_Structure`, `05_Doors`.
- **Authoritative files**: `Assets/Editor/FrozenTrainUnifiedAuthoring.cs` (rebuild order), `FrozenTrainLayout.cs` (dimensions), `FrozenTrainBuilder.cs` (base), `FrozenTrainLongVestibuleArchitecture.cs` (per-car room layout), `FrozenTrainMinimalInteriorPass.cs` (final removal/materials and scene refresh). Gameplay in `Assets/Scripts/`.

## Token-efficient procedure
1. Search exact source symbols; fetch only necessary code ranges (normally 1–3 files). Do not inventory the entire repo by default, and don't read all of `PROJECT_MEMORY.md` each turn. Use its relevant/latest section only for prior decisions.
2. Make one coherent patch; preserve existing objects, E interaction, modular car geometry and rebuild order. Never add parallel authoring systems.
3. Check changed code, obvious missing `using` and symbol references; don't confuse static checks with Unity compilation.
4. If geometry changed, remind: `Frozen Train > Rebuild Authored Train` then save. Respond with outcome + commit + only necessary caveats.

Adaptive 1600×900 QA images are taken on Editor Play into `Docs/Screenshots/`, replacing old local images. **They must be committed/pushed before remote GitHub review**. Don't treat old pushed screenshots as current.

Claude-specific scoped rules and skills live in `.claude/`; art procedure: `Docs/ArtPipeline.md`; UV/PBR plan: `Docs/TEXTURE_WORKFLOW.md`; archived decisions: `PROJECT_MEMORY.md`. Do not read them unless relevant.
