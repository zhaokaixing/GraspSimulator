# Resources

简要（中文）
- 本目录用于存放 Unity 可通过 Resources.Load 加载的运行时资源（如贴图、材质、预制体、文本资源等）。
- 使用建议：
  - 仅将确实需要在运行时动态加载的资源放入 Resources，避免滥用导致打包体积增大。
  - 始终提交对应的 .meta 文件以保持 GUID 稳定。
  - 优先使用 Addressable 或 AssetBundle 做大规模资源管理，Resources 用作简单、少量的动态加载场景。
  - 在修改资源后，在 Unity 中确认加载路径和名称一致。
- 维护：
  - 定期检查未被使用的资源并清理，减少包体大小。

Brief (English)
- This folder contains runtime assets meant to be loaded via Resources.Load (textures, materials, prefabs, text files).
- Guidelines: only keep assets here that must be dynamically loaded; commit .meta; prefer Addressables for larger projects.
