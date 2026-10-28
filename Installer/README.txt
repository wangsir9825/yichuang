译窗安装器构建

1. 先运行 Build-LlamaRuntime.ps1 获取并校验锁定版本的官方 llama.cpp 运行包。
   再运行 Build.ps1 和 Verify.ps1，确认 App 是本次要分发的 1.5.9 版本。
   内置组件来源、许可与哈希见 llama-runtime-notes.txt、App/llama/runtime-manifest.json。
2. 安装或解压 Inno Setup 6.7+，将 ISCC.exe 路径传给 Build-Installer.ps1 -CompilerPath。
   本次使用 Tools.InnoSetup NuGet 6.7.3 中未修改且签名有效的编译器。
3. 准备 Build/Prerequisites/dotnet48.exe：
   微软 .NET Framework 4.8 官方离线安装程序：
   https://dotnet.microsoft.com/download/dotnet-framework/thank-you/net48-offline-installer
   原始下载：https://go.microsoft.com/fwlink/?linkid=2088631
4. 准备 Build/Prerequisites/vc_redist.x64.exe：
   微软 Visual C++ 2015-2022/后续兼容版本 x64 官方可再分发安装程序。
   https://learn.microsoft.com/cpp/windows/latest-supported-vc-redist
   本次复用本机 Visual Studio 附带的微软签名原件。
5. 运行 Build-Installer.ps1。脚本验证微软 Authenticode 签名及内置 llama.cpp 各文件哈希，记录载荷哈希，并拒绝覆盖已有安装包。

安装器默认安装到当前用户的 LocalAppData/Programs/YiChuang。
仅缺运行环境时请求管理员权限；重启请求会阻止提前启动应用。
卸载只处理安装器记录的应用文件；不清除用户配置、Ollama、模型缓存或共享运行环境。
安装包未签署开发者证书。Windows 的来源/信誉提示由用户和系统决定。
禁止将私人配置、模型缓存、备份、构建工具缓存和测试日志加入载荷。
App/tessdata 中两份小型 OCR 语言数据是离线 OCR 必需资源，属于有意包含的公开依赖。
