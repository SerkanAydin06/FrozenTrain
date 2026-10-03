---
paths:
  - "Assets/**/*.unity"
  - "Assets/**/*.prefab"
  - "Assets/**/*.mat"
  - "Assets/**/*.asset"
---
# Unity serialized asset rules

- Avoid reading or rewriting entire large Unity YAML assets unless the task specifically requires serialized data.
- For train layout, prefer the editor authoring source and `Frozen Train > Rebuild Authored Train` instead of hand-editing generated prefab/scene YAML.
- Preserve GUID/fileID references when a serialized asset must be edited.
- Never regenerate unrelated `.meta` files.
- Inspect only the relevant serialized object/hunk when possible.