# MonoBleedingEdge

简要（中文）
- 本目录包含 Unity 使用的 Mono 运行时相关文件（第三方运行时/托管库）。
- 注意事项：
  - 这些文件通常为二进制与运行时支持文件，不要手动编辑。
  - 确认许可证与分发要求（若发布或分发，保留相应 LICENSE）。
  - 若需升级 Mono 版本，在独立分支上验证兼容性与 Managed 插件运行情况。
- 建议：
  - 不将大型、可重建的运行时二进制直接纳入主要源码分支，优先放 release 或外部存储。
  - 如需记录版本，新增 VERSION 或 CHANGELOG 文件说明来源与校验信息。

Brief (English)
- This folder contains Mono runtime files used by Unity (managed/binary runtime support).
- Do not edit binaries. Verify license and distribution terms when distributing builds.
- Test any runtime upgrades on a separate branch before merging to main.
