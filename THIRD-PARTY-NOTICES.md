# 第三方与许可说明

译窗基于既有 Xtranslator 重建工程继续开发，保留既有版权信息；项目整体“源码公开，尚未指定开放授权许可证”，没有为旧代码或第三方库另授许可。

- llama.cpp：MIT；运行包及 OpenMP 许可在 App/llama，版本与哈希见 runtime-manifest.json。
- 独立 OCR：Tesseract / .NET wrapper / tessdata_fast 为 Apache-2.0，Leptonica 为 BSD，InteropDotNet 为 MIT；原许可位于 App/Licenses。
- AutoHotkey 1.1.33.02：GPL-2.0 及上游附带组件条款。三个取词辅助程序中的 AHK 脚本见 Source/Scripts；原始运行时对应源码和许可随 Release 单独提供。项目整体的许可状态不改变这些组件的权利。
- Newtonsoft.Json：MIT，原版权声明随对应上游许可证提供。
- 原工程还依赖 DSkin 16.6.3.38、Noesis.Javascript、WMPLib 互操作组件和 wcocr 桥接库；当前档案未附齐这些旧依赖的许可来源，不能据此宣称其拥有统一开放授权。
- 微软 .NET / Visual C++ 运行环境为未修改的再分发程序，适用微软随附条款。
- 安装器使用 Inno Setup。Ollama、翻译模型、微信 OCR 引擎/模型不随包分发。用户另行获取时遵守各自条款。

二次分发或商业集成前应核实各第三方组件许可，不将此工程名称或源码公开理解为对第三方权利的替代。
