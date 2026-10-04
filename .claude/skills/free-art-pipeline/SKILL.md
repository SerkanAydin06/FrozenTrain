---
name: free-art-pipeline
description: Use when creating, sourcing, replacing, importing, rigging, texturing, or integrating FrozenTrain 3D assets, characters, props, materials, textures, or environment art without paid tools.
---
# Free art pipeline

Start with:

1. `Docs/ArtPipeline.md`
2. `ArtSource/AssetLedger.csv`
3. only the specific train/layout file that will receive the asset

## Decision sequence

For each requested asset:

1. Assign a stable `FT_*` asset ID.
2. Search `Assets/Art/` and the ledger for reuse.
3. If missing, choose one route:
   - verified free/CC0 source;
   - AI-written Blender Python for train-specific hard-surface props;
   - MakeHuman/MPFB + Mixamo for humanoid bases/rig/animation;
   - keep current greybox if no free legal route is good enough.
4. Create/update `ArtSource/Briefs/<asset-id>.md` when generating custom art.
5. Record third-party sources/licenses in the ledger.
6. Put editable/source files under `ArtSource/`, runtime imports under `Assets/Art/`.
7. Integrate the visual as a replaceable child of the stable authored/gameplay object.
8. Do not create a parallel train layout or hand-place changes that the builder will erase.
9. Rebuild train if required and review only the affected screenshots.

## Quality targets

- stylized-realistic industrial survival;
- URP materials;
- reuse tileable PBR/trim sheets;
- 512–1024 textures for small props, 2048 for hero assets/characters;
- no unnecessary 4K textures;
- simple colliders;
- believable silhouette before micro-detail;
- no asset may block corridor, ladder, connector, window, or interaction access.

## User burden

Do not instruct the user to learn Blender, sculpt, rig, UV, or texture manually. If Blender is required, prefer deterministic scripts/CLI steps that an AI can generate and the user can run without artistic knowledge.

## Verification

- `git diff --check`
- confirm license ledger entry for third-party content
- confirm no missing materials/textures
- confirm prefab scale/orientation/pivot
- confirm interaction wrapper survived
- visual screenshot comparison after rebuild/play
