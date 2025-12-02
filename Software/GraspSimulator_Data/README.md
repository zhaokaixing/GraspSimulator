# GraspSimulator_Data

简要（中文）
- 该目录为 Unity Player 打包后的运行时数据文件夹，包含场景资源、sharedassets、runtime 数据与 Managed/Plugins 等。
- 注意事项：
  - 不要手动编辑二进制文件（*.assets, *.resS, *.sharedassets 等）。
  - 若需要调试或提取资源，请使用专用工具（如 Unity/AssetStudio/TriLib），并在独立工作目录操作。
  - 此目录通常较大，请在仓库中谨慎管理（推荐将可重新生成的大文件放到 release 或外部存储）。
- 建议：
  - 将可重建资源（可编译的输出）排除在版本控制之外，使用 .gitignore 保存运行时可重建文件。
  - 对于需要保留的配置或小体积运行数据，添加对应说明文件（LICENSE/NOTICE/README）并注明来源与版本。

Brief (English)
- This folder contains Unity build runtime data (scenes, sharedassets, resources, Managed/Plugins).
- Do not edit binary files here. Use appropriate tooling to inspect or extract assets.
- Prefer excluding large build outputs from git; keep only small, necessary metadata or license notes.