---
paths:
  - "Assets/Art/**"
  - "ArtSource/**"
  - "Docs/ArtPipeline.md"
  - "Assets/Editor/FrozenTrainLayout.cs"
  - "Assets/Editor/FrozenTrainBuilder.cs"
  - "Assets/Editor/FrozenTrainProductionArtPro.cs"
---
# Free-only art production rules

- Read `Docs/ArtPipeline.md` before adding or replacing production art.
- The project owner is not expected to model, UV, rig, or texture by hand.
- Do not require paid 3D-generation services, paid asset packs, or paid plugins.
- Prefer this order: existing FrozenTrain asset -> verified free/CC0 asset -> AI-written Blender Python asset -> MakeHuman/MPFB + Mixamo for humanoids -> keep greybox until a free route exists.
- Every third-party asset must have source/license recorded in `ArtSource/AssetLedger.csv`.
- Reject unclear licenses, ripped assets, editorial-only assets, and non-commercial licenses.
- Source/editable working files belong under `ArtSource/`; Unity-ready runtime files belong under `Assets/Art/`.
- Do not hand-edit generated train prefabs in a way that `Rebuild Authored Train` will erase.
- `FrozenTrainLayout.cs` + `FrozenTrainBuilder.cs` remain the only architecture/placement source of truth.
- `FrozenTrainProductionArtPro.cs` is the single production-art pass (called directly by `FrozenTrainUnifiedAuthoring`); do not create more stacked production-art passes.
- Production art may run after the canonical builder only as a visual skin/detail/material pass; it must not move architecture, change car/deck dimensions, or create a parallel layout system.
- Keep gameplay/interactable components on stable wrapper objects; art meshes should be replaceable children.
- Use URP-compatible materials, modest texture sizes, shared trim/tile materials, and simple colliders.
- Procedurally generated textures/materials are allowed when deterministic and source-controlled by code; put generated Unity assets under `Assets/Art/`.
- Visual changes must preserve corridors, ladders, connector doors, window sight-lines and interaction access.
- For visual changes, compare the affected automatic screenshots after rebuilding/playing, preferably through the GitHub screenshot artifact.
