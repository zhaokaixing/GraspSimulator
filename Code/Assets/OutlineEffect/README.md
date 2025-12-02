# OutlineEffect
Brief
- This folder contains scripts and example resources for rendering object outlines in Unity.
- Main files:
  - LinkedSet.cs — helper collection for managing linked vertices/boundaries.
  - Outline.cs — component to attach to GameObjects to configure outline color/width and trigger rendering.
  - OutlineEffect.cs — global manager that handles registration/unregistration and interacts with rendering pipeline.
  - Demo/, Resources/ — sample scenes and runtime assets (shaders, materials).

Usage & Maintenance
- Edit script parameters and materials in the Unity Editor; avoid manual edits to binary/.meta files.
- After script changes, recompile and test in Unity; pay attention to registration order and material properties.
- Commit new shaders/materials together with their .meta files to preserve GUIDs.
- For performance: minimize per-frame register/unregister calls and reuse materials / cached computations.
