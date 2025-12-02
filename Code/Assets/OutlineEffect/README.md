# OutlineEffect

简要（中文）
- 本目录包含用于在 Unity 中绘制对象轮廓（outline）的脚本与示例资源。
- 主要文件：
  - LinkedSet.cs — 内部辅助集合类，用于管理顶点/边界链接（维护实现细节）。
  - Outline.cs — 绑定到 GameObject 的组件，用于配置轮廓颜色、宽度等并触发渲染。
  - OutlineEffect.cs — 管理全局 Outline 特效，处理注册/注销与渲染管线交互。
  - Demo/、Resources/ — 示例场景与运行时资源（如材质、着色器等）。

使用与维护建议
- 优先在 Unity Editor 中修改脚本参数与材质，不要手动编辑二进制或 .meta 文件。
- 修改脚本后在 Unity 中重新编译并测试场景，注意 OutlineEffect 的注册顺序与材质属性。
- 若添加新资源（shader/material），同时提交对应的 .meta 文件以保留 GUID。
- 对于性能调优：尽量减少每帧注册/注销操作，复用材质与缓存计算结果。

Brief (English)
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
