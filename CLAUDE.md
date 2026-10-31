# FrozenTrain — Claude Code (quick context)

**Task first:** answer in concise Turkish; use connected GitHub tools for direct changes when available. Inspect current source, not chat memory. Do not invent successful Unity compilation, visuals or screenshots.

## Current contract (2026-10-31)
- Unity 6.3 LTS / URP / Windows; scene: `Assets/Scenes/FPSPrototype.unity`.
- Exactly 5 separate coupled cars (Locomotive, Generator, Living, Workshop, Greenhouse), each **24 × 9.2 m**, 3 decks; car-centre spacing **25 m** with a 0.8 m gap and a short **main-deck-only gangway** per joint. Locomotive faces **-X**.
- Middle deck: private locomotive cab; cars 2–5 have straight **+Z public corridor** (Z≈1.90…4.40) and an opaque left functional compartment. Lower = technical, upper = accommodation.
- Vertical access is **inside cars** (`CarAccess`, Builder): every car has one wall-mounted E ladder to the upper deck at the rear (+X) end of the +Z corridor beside the rear end door; only the locomotive has a technical-deck ladder. Lower/upper decks never connect between cars. No stair rooms or decorative flights.
- **Current look is deliberately bare and UNTEXTURED.** Keep structural shell, windows, simple aligned functional doors, vertical ladders, essential lighting and minimal exterior. Do not restore removed props, orange trim or old patterned finishes. Future PBR slots: `Assets/Art/Materials/TrainBaseline/{01_Body,02_Walls,03_Floors,04_Structure,05_Doors}.mat`. See `Docs/TEXTURE_WORKFLOW.md`.

## Sources — open only what this task needs
| Area | Source of truth |
|---|---|
| Rebuild/order | `Assets/Editor/FrozenTrainUnifiedAuthoring.cs` |
| Dimensions/layout | `Assets/Editor/FrozenTrainLayout.cs` |
| Base geometry/scene | `Assets/Editor/FrozenTrainBuilder.cs` |
| Doors, ladders, gangway | `Assets/Editor/FrozenTrainBuilder.cs` (`BuildCarAccess`, `BuildConnectorPrefab`) |
| Per-car room layout | `Assets/Editor/FrozenTrainLongVestibuleArchitecture.cs` |
| Final clean-up/material slots/scene refresh | `Assets/Editor/FrozenTrainMinimalInteriorPass.cs` |
| Interaction | `Assets/Scripts/Interaction/` |
| Survival/state | `Assets/Scripts/Management/TrainPrototypeState.cs`, `SurvivalSimulation.cs` |
| UI | `Assets/Scripts/Management/TrainManagementPanel.cs` |
| Screenshots | `Assets/Scripts/DevTools/TrainScreenshotCapture.cs` |

Rebuild chain: Builder → ProductionArtPro → ExteriorScenery → MainStreet* → LongVestibule → MinimalInterior; **the minimal pass is final authority**. Legacy QA/polish passes were deleted 2026-10-31; do not recreate them or old in-car stair generators. Final pass refreshes scene prefab instances: keep this.

## Low-cost workflow
1. For repo tasks, check status/current branch; search **exact filenames/symbols** and read relevant ranges (typically 1–3 files). Do not read this project's whole archive on every task.
2. Check `PROJECT_MEMORY.md` **heading index / most recent relevant section only when needed**, not all prior logs; append only a new long-lived decision or significant unresolved issue (not every routine fix).
3. Make the smallest coherent patch. Reuse components; no duplicate systems, extra postprocessor passes or speculative story/NPC changes.
4. Verify once: changed diff, whitespace, impacted symbols and compilation risks. Unity compilation/Play Mode must be reported as **unverified** if not actually run.
5. After geometry changes: pull → `Frozen Train > Rebuild Authored Train` → save scene. Provide commit link, compact outcome and any genuine blocker. No repetitive status prose.

## Media and assets
- Existing/free/CC0 or reproducible Blender scripts preferred; no paid asset requirement. Third-party sources/licenses → `ArtSource/AssetLedger.csv`. Check `Docs/ArtPipeline.md` only on art tasks.
- In Editor Play of `FPSPrototype`, QA screenshots are **on by default** (toggle `Tools > FrozenTrain > Capture Screenshots on Play`). Separate camera; adaptive points; latest-only local `Docs/Screenshots/`. **Local files are NOT visible on GitHub until pushed.** Never claim pixel-level verification from stale screenshots.
- Scene/prefab YAML, `.meta`, `Library/`, `Temp/`, `Logs/`, `obj/` should not be broadly read unless the task needs them.

## On-demand rules
Claude path-scoped rules in `.claude/rules/` and skills in `.claude/skills/` apply only to relevant tasks. Prefer them over expanding this file. History (not required upfront): `PROJECT_MEMORY.md`.
