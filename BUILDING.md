# 构建

Windows x64，.NET Framework 4.8，Visual Studio 2022 Build Tools（含 Roslyn C# 12 和 .NET 桌面构建工具）。

1. 运行 `powershell -NoProfile -ExecutionPolicy Bypass -File .\Restore-Dependencies.ps1`。它只下载本版本便携包、核对 SHA-256，并提取 App 依赖与基准程序；不包含私人配置，也不下载翻译模型。
2. 退出本源码目录启动的译窗。运行 `powershell -NoProfile -ExecutionPolicy Bypass -File .\Build.ps1`，必要时添加 `-CompilerPath <Roslyn csc.exe>`。
3. 运行 `powershell -NoProfile -ExecutionPolicy Bypass -File .\Verify.ps1`，也支持 `-CompilerPath`。测试使用独立目录与模拟服务，输出在 Evidence，不等同于全部线上服务验收。
4. 构建安装器需另外准备 Inno Setup 6.7+ 和微软签名运行环境安装程序，见 Installer/README.txt；不会把这些下载工具打入源码。

仓库不提交二进制依赖与构建产物。依赖哈希见 DEPENDENCIES.json；AutoHotkey 运行时来自 1.1.33.02，上游对应源码另附 Release。Build-InputHelpers.ps1 将本仓库 AHK 脚本写入原运行时资源。旧工程还使用 DSkin、Noesis 等原有 DLL，公开源码不额外授予这些第三方库的许可。

本版本安装包由 1.5.4 对应源码构建，随后在独立副本复建和回归；重新编译不保证逐字节一致。不得将任意来源 DLL 替换成同名文件后声称仍为本次核验版本。

测试时请关闭所有译窗实例；目录较深时，使用 `-EvidenceDirectory <较短的隔离目录>`，防止测试配置路径超过 Windows 传统路径限制。
