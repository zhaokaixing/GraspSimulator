Brief
- This folder contains Unity prefabs and their .meta files (note folder name is "Prefebs").
- Example:
  - contactPoint.prefab — prefab for contact point visualization.
- Guidelines:
  - Edit prefabs in Unity Prefab Mode; do not manually edit YAML/.meta when possible.
  - Always commit both .prefab and .meta to preserve GUIDs.
  - Organize prefabs into subfolders (e.g. hand, ui) and use descriptive, lowercase names with underscores.
  - Make small, well-documented commits for changes that affect physics/colliders/rigging.
