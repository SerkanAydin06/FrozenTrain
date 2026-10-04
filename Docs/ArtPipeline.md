# FrozenTrain — Free-Only Art Production Pipeline

## Purpose

FrozenTrain must be buildable without paid 3D-generation services and without requiring the project owner to know Blender, sculpting, UV editing, rigging, or texture painting.

AI assistants working on this repository must treat art production as an engineering pipeline, not as a request for the user to manually model assets.

## Hard rules

- Do **not** require Meshy, Tripo, paid asset packs, paid plugins, or paid AI credits.
- Do **not** tell the user to learn Blender before progress can continue.
- Blender may be used by AI-generated Python scripts or very small deterministic operations; the user should not need to model by hand.
- Prefer assets with clear commercial-use licenses. CC0/public-domain is preferred.
- Never use ripped game assets, unclear-license downloads, editorial-only assets, or non-commercial licenses.
- Keep the current `FrozenTrainLayout.cs` + `FrozenTrainBuilder.cs` architecture as the source of placement and gameplay binding. Art replaces primitive visuals; it must not create a parallel layout system.
- A missing final asset must fall back to the existing prototype shape rather than blocking gameplay.

## Art direction

Target: **stylized-realistic industrial survival**, optimized for Unity URP and modest PCs.

- Readable silhouettes and believable proportions matter more than micro-detail.
- Heavy industrial metal, insulation, rubber, fabric, frost, grime and warm practical lights.
- Avoid photoreal asset requirements that force sculpting-heavy workflows.
- Reuse trim sheets, tileable PBR materials and modular parts.
- Keep train interiors visually coherent even when assets come from different free sources.

## The sourcing decision tree

For every requested asset, use the first viable route below.

### Route A — Reuse an existing FrozenTrain asset

1. Search `Assets/Art/` and `ArtSource/AssetLedger.csv`.
2. Reuse or create a variant before adding another near-duplicate.
3. Preserve collider/interactable behavior through the existing authored wrapper object.

### Route B — Use a free licensed asset

Preferred sources:

- **Poly Haven** — CC0 textures, HDRIs and 3D assets.
- **Kenney** — CC0 game assets.
- Other sources are allowed only when the individual asset license is explicit and compatible with commercial games.

When importing anything third-party:

1. Record it in `ArtSource/AssetLedger.csv` before or with the asset commit.
2. Keep the original source URL and license URL.
3. Normalize scale, pivot, materials and naming for FrozenTrain.
4. Do not let mixed-source art styles remain visibly inconsistent; recolor/re-material it to the FrozenTrain palette.

### Route C — Generate hard-surface assets with Blender Python

Use this for train-specific objects such as consoles, seats, bunks, cabinets, tanks, pipes, generators, workbenches and hydroponic modules when no suitable free asset exists.

Workflow:

1. Write a short asset brief in `ArtSource/Briefs/<asset-id>.md`.
2. If useful, create a 2D concept/reference image first.
3. Have the AI write a deterministic Blender Python script in `ArtSource/Blender/<asset-id>.py`.
4. Run Blender headlessly or through its scripting workspace; the script creates the model, UVs/basic materials and exports FBX/GLB.
5. Save the editable `.blend` source under `ArtSource/Blender/Source/` when practical.
6. Import the exported model to `Assets/Art/Models/` and make a prefab in `Assets/Art/Prefabs/`.
7. Replace only the visual portion of the authored prototype object; keep gameplay bindings on the stable wrapper.

The user is not expected to edit the mesh manually. If a generated mesh needs extensive artistic repair, prefer a different free source or simplify the design.

### Route D — Human characters without paid services

Base human pipeline:

1. **MakeHuman / MPFB** for a free/open human base.
2. Keep a small set of body bases; create variety with face/body parameters, hair, skin, clothing colors and accessories.
3. Use simple modular winter workwear. Prefer free licensed clothing or AI-generated Blender scripts for simple jacket/pants/boots shells.
4. **Mixamo** may be used for biped auto-rigging and free animation acquisition when appropriate.
5. Export to Unity Humanoid and create reusable Animator Controllers / Avatar mappings.
6. NPC visual variety should come from modular combinations, not dozens of unique high-cost characters.

Initial character set should be small: worker base male/female body variants, technician outfit, greenhouse outfit, general passenger outfit, a few hair/head variants.

## Texture and material pipeline

Preferred order:

1. Existing FrozenTrain PBR material.
2. CC0 tileable PBR texture from Poly Haven or another verified CC0 source.
3. Procedural Blender shader/material baked to textures when a custom surface is needed.
4. AI-generated 2D texture only when licensing/use is clear and it can be made tileable/reviewed.

Target maps when useful:

- Base Color / Albedo
- Normal
- Metallic
- Roughness or Smoothness
- Ambient Occlusion
- Emission for screens/lights

Do not create unique 4K textures for every prop.

Recommended budgets:

- small prop: 512–1024 texture set
- medium reusable prop: 1024
- hero console / character: 2048
- shared wall/floor/trim material: 1024–2048 tileable
- 4K only with a demonstrated visual need

## Geometry budgets

These are guidance, not hard limits:

- small prop: 500–5,000 triangles
- medium prop: 3,000–15,000 triangles
- hero machinery / cockpit module: 10,000–40,000 triangles
- humanoid including clothing/hair: roughly 20,000–50,000 triangles

Prefer normal maps and material detail over excessive geometry. Fine exterior snow/ice detail should not receive colliders.

## Folder contract

Source/editable art lives outside Unity's `Assets` tree when Unity does not need to import it:

```text
ArtSource/
  Briefs/                 asset briefs for humans and AIs
  References/             concept/reference images
  Blender/                AI-written Blender scripts
    Source/               optional .blend source files
  Exports/                temporary FBX/GLB exports before Unity import
  AssetLedger.csv         license/source ledger
```

Runtime/imported art:

```text
Assets/Art/
  Models/
    Train/
    Props/
    Characters/
    Environment/
  Textures/
  Materials/
  Prefabs/
    Train/
    Props/
    Characters/
    Environment/
```

Do not put raw working files, reference dumps or downloaded archives under `Assets/`.

## Asset IDs and naming

Use stable IDs so an asset can be replaced without changing gameplay logic.

Examples:

- `FT_SEAT_Living_Double_A`
- `FT_TABLE_Living_4P_A`
- `FT_BED_Upper_Single_A`
- `FT_GEN_MainGenerator_A`
- `FT_PROP_ElectricalCabinet_A`
- `FT_GREEN_HydroBed_A`
- `FT_CHAR_Worker_Base_M_A`

Unity prefabs use the same ID plus `.prefab`.

## Integration with the authored train

The train builder remains responsible for position, gameplay state, colliders that matter to navigation, and `E` interaction bindings.

When a production art prefab is ready:

1. Keep the existing logical/authored object as the stable wrapper.
2. Attach/instantiate the art prefab as its visual child or extend the builder to resolve the art asset by stable ID.
3. Remove/disable only the prototype visual geometry that the art replaces.
4. Do not move the interaction component onto fragile mesh children.
5. Do not hand-edit generated car prefabs in a way that will be erased by `Rebuild Authored Train`.
6. Rebuild and visually verify the affected car/deck with `Docs/Screenshots`.

## Definition of done for an art asset

An asset is production-ready only when:

- source/license is recorded if third-party;
- scale and orientation are correct in Unity;
- pivot is useful;
- material uses URP-compatible shaders;
- no missing textures/pink materials;
- collider strategy is intentional;
- interaction wrapper remains stable if interactable;
- it does not block corridors, doors, ladders or windows;
- visual style matches FrozenTrain;
- screenshot review shows a clear improvement over the prototype.

## Character definition of done

Additionally:

- Humanoid rig maps correctly;
- idle/walk animations do not deform badly;
- hands/feet are acceptable at gameplay camera distance;
- clothing does not visibly intersect the body during basic motion;
- one shared rig/animation library is reused where possible.

## AI behavior when an asset is requested

The AI should respond internally in this order:

1. Identify asset ID and gameplay purpose.
2. Search the existing art + ledger.
3. Decide `reuse`, `free-source`, `Blender-script`, or `MakeHuman/MPFB character`.
4. Create/update the brief and ledger.
5. Produce/import the asset.
6. Integrate it without breaking builder/layout logic.
7. Verify with targeted screenshots.

Do not stop at a concept image if the request is for an in-game 3D asset. A concept is an intermediate artifact, not completion.

## Free tool baseline

- Blender — free/open-source 3D creation and Python automation.
- MakeHuman / MPFB — free/open-source human creation; core graphical assets are intended for permissive/CC0 use.
- Mixamo — free with an Adobe ID for supported biped rigging/animations; check current service terms before production use.
- Poly Haven — CC0 textures/HDRIs/models.
- Kenney — CC0 game assets.
- Unity 6.3 LTS + URP — runtime target.

If a listed service changes its license or pricing, do not silently substitute a paid service. Use another free route and update this document.
