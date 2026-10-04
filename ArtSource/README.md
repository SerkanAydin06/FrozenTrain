# FrozenTrain ArtSource

This folder contains **source/editable art work**, not Unity runtime assets.

The project owner is not expected to manually model or texture assets. AI assistants should use this area for reproducible briefs, scripts, references and license tracking.

Expected structure as art is added:

```text
ArtSource/
  Briefs/       one short brief per custom asset
  References/   concept/reference images
  Blender/      AI-written Blender Python scripts
    Source/     optional .blend files
  Exports/      temporary FBX/GLB output before Unity import
  AssetLedger.csv
```

Unity-ready files belong under `Assets/Art/`, not here.

## Rules

- Never commit downloaded archives/executables here.
- Prefer reproducible scripts and small source files.
- Every third-party model, texture, HDRI, sound or animation must be recorded in `AssetLedger.csv`.
- Generated/self-authored assets should also receive an asset ID and brief when non-trivial.
- Paid-only services are not part of the FrozenTrain production pipeline.

See `Docs/ArtPipeline.md` for the complete workflow.
