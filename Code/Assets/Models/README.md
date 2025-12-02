# Assets/Models
Brief
- This folder contains 3D models used by the project and their Unity .meta files.
- Example subfolders:
  - ExsampleObjects/ — example scene objects
  - HandR_v1/ — robotic hand model and assets
- Guidelines:
  - Import models into Unity under Assets/Models/<name>/ and keep .meta files committed to preserve GUIDs.
  - Recommended formats: FBX, OBJ, glTF. Prefer FBX for rigs/animations.
  - Adjust scale, pivot and materials in the Unity Editor; do not edit .meta/YAML manually.
  - Naming: use lowercase with underscores, avoid spaces and special characters.
- Maintenance:
  - Always commit both the model and its .meta file when adding/updating resources.
  - For mesh/rig changes, re-export from your 3D tool and re-import into Unity.
