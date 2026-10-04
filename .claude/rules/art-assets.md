---
paths:
  - "Assets/Art/**"
  - "ArtSource/**"
  - "Docs/ArtPipeline.md"
  - "Assets/Editor/FrozenTrainLayout.cs"
  - "Assets/Editor/FrozenTrainBuilder.cs"
---
# Free-only art production rules

- Read `Docs/ArtPipeline.md` before adding or replacing production art.
- The project owner is not expected to model, UV, rig, or texture by hand.
- Do not require paid 3D-generation services, paid asset packs, or paid plugins.
- Prefer this order: existing FrozenTrain asset -> verified free/CC0 asset -> AI-written Blender Python asset -> MakeHuman/MPFB + Mixamo for humanoids -> keep greybox until a free route exists.
- Every third-party asset must have source/license recorded in `ArtSource/AssetLedger.csv`.
- Reject unclear licenses, ripped assets, editorial-only assets, and non-commercial licenses.
- Source/editable working files belong under `ArtSource/`; Unity-ready runtime files belong under `Assets/Art/`.
- Do not hand-edit generated train prefabs in a way that `Rebuild Authored Train` will erase. Integrate production visuals through the layout/builder architecture.
- Keep gameplay/interactable components on stable wrapper objects; art meshes should be replaceable children.
- Use URP-compatible materials, modest texture sizes, shared trim/tile materials, and simple colliders.
- Visual changes must preserve corridors, ladders, connector doors, window sight-lines and interaction access.
- For visual changes, compare the affected automatic screenshots after rebuilding/playing.
