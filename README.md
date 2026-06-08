# Diff-Lite WPF

**轻量级代码比对工具** — 原生 Windows 应用，基于 C# WPF + AvalonEdit。

![platform](https://img.shields.io/badge/platform-Windows-blue)
![dotnet](https://img.shields.io/badge/.NET-8.0-purple)

## 功能特性

- 🖥️ **纯中文界面** — 所有菜单、按钮、提示均为简体中文
- 🌙 **深色/浅色主题** — VS Code 风格的现代化界面
- 📂 **文件比对** — 打开两个文本/代码文件进行左右分屏比对
- 🎯 **差异高亮** — 新增行标绿、删除行标红
- 🔍 **代码高亮** — 基于 AvalonEdit，支持多种编程语言
- 🚀 **极致轻量** — 安装包仅 ~5MB，运行时内存 ~30MB
- ⌨️ **快捷键支持** — Ctrl+O 打开文件，Ctrl+R 一键比对
- 🛡️ **防御性编程** — 所有错误以中文弹窗提示

## 系统要求

- Windows 10 / Windows 11
- .NET 8.0 运行时（如未安装，首次运行会自动提示安装）

## 开发环境

```bash
# 需要 .NET 8.0 SDK
dotnet --version  # 应为 8.0.x

# 还原依赖
cd diff-lite-wpf
dotnet restore

# 构建
dotnet build

# 运行
dotnet run

# 发布单文件
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## 键盘快捷键

| 快捷键 | 功能 |
|--------|------|
| Ctrl+O | 打开左侧文件 |
| Ctrl+Shift+O | 打开右侧文件 |
| Ctrl+R | 一键比对 |
| Ctrl+Shift+R | 清空内容 |
| Ctrl+D | 切换深色/浅色模式 |

## 技术栈

- **框架**: .NET 8.0 WPF
- **编辑器**: AvalonEdit (WPF 原生代码编辑器)
- **比对引擎**: DiffPlex (.NET 差异比对库)
- **打包**: dotnet publish 单文件发布

## 与 Electron 版对比

| | **WPF 版（新）** | Electron 版（旧） |
|---|---|---|
| 安装包大小 | **~5MB** | ~100MB |
| 安装后大小 | **~15MB** | ~300MB |
| 运行时内存 | **~30MB** | ~150-300MB |
| 启动速度 | **瞬间** | 2-5秒 |
