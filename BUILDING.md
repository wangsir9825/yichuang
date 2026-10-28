# 构建当前主分支（1.5.9）

需要 Windows x64、.NET Framework 4.8、Visual Studio 2022 Build Tools（Roslyn C# 12 和 .NET 桌面构建工具）。

1. 运行 `powershell -NoProfile -ExecutionPolicy Bypass -File .\Restore-Dependencies.ps1`。依赖仍锁定到公开的 **1.5.5 便携包**，脚本校验 DEPENDENCIES.json 中的 SHA-256，再提取 App；它恢复的基准 EXE 是 1.5.5，不能直接把它当作当前源码的构建结果。脚本不下载翻译模型。
2. 运行 `powershell -NoProfile -ExecutionPolicy Bypass -File .\Build.ps1`，用当前源码重新构建主程序、OCR 和取词工具。可使用 `-CompilerPath <Roslyn csc.exe>` 指定编译器。构建完成的主程序版本为 1.5.9.0。
3. 退出所有译窗实例，再运行 `powershell -NoProfile -ExecutionPolicy Bypass -File .\Verify.ps1`。可指定 `-CompilerPath`、`-AppDirectory`、`-EvidenceDirectory`；长路径环境宜选择较短的证据目录。脚本含 28 组回归及跨进程检查，会使用隔离配置/模拟服务和真实 WinForms 控件，不能等同于全部线上功能验收。其他译窗实例可能干扰取词/OCR 通信。
4. 安装器构建需 Inno Setup 6.7+、微软签名运行库安装程序和固定 llama.cpp 组件，详见 Installer/README.txt。先验证应用，再运行 Build-Installer.ps1。该脚本拒绝覆盖已有同名安装包。

仓库不提交二进制依赖、个人配置、模型或构建输出。Restore-Dependencies.ps1 使用历史公开包复用不变的依赖；不要把下载地址机械改成尚未发布的 1.5.9。AutoHotkey 运行时为 1.1.33.02，对应上游源码在 Releases；Build-InputHelpers.ps1 将本仓库 AHK 脚本写入原运行时资源。DSkin、Noesis 等旧依赖的许可状态见 THIRD-PARTY-NOTICES.md。

Tools/RuntimeCheck.cs 为可选只读诊断工具，不自动随主程序构建。可用 .NET Framework 64 位 csc 编译，引用 System.Windows.Forms、System.Drawing、System.Management；Tests/RuntimeCheckLogRegression.cs 可独立编译并传入该工具路径，验证日志筛选及用户路径脱敏。

此前版本及本次同步的验证范围见 VALIDATION.md。重新编译不保证输出文件逐字节一致。
