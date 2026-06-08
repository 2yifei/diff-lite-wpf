# Diff-Lite

**轻量级代码比对工具** — 原生 Windows 应用，基于 C# WPF + AvalonEdit。

![version](https://img.shields.io/badge/version-2.0.0-blue)
![platform](https://img.shields.io/badge/platform-Windows-purple)
![dotnet](https://img.shields.io/badge/.NET-8.0-512BD4)

## 功能特性

- 🖥️ **纯中文界面** — 所有菜单、按钮、提示均为简体中文
- 🌙 **深色/浅色主题** — VS Code 风格的现代化界面，一键切换
- 📂 **文件比对** — 打开两个文本/代码文件进行左右分屏比对
- 🎯 **差异高亮** — 新增行标绿、删除行标红、修改行标黄
- 🔍 **语法高亮** — 基于 AvalonEdit，支持 20+ 种编程语言自动识别
- 🚀 **极致轻量** — 安装包 < 1MB，运行时内存 ~30MB
- ⌨️ **快捷键支持** — Ctrl+O 打开文件，Ctrl+R 一键比对
- 🛡️ **防御性编程** — 所有错误以中文弹窗提示

## 系统要求

- Windows 10 / Windows 11
- [.NET 8.0 桌面运行时](https://dotnet.microsoft.com/download/dotnet/8.0)（通常已自带）

## 下载

前往 [Releases](https://github.com/2yifei/diff-lite-wpf/releases) 下载最新 `DiffLite-WPF-v2.0.x.zip`，解压后双击 `DiffLite.exe` 运行。

## 开发指南

```bash
# 安装 .NET 8.0 SDK
dotnet --version   # 应为 8.0.x

# 还原依赖
dotnet restore

# 编译
dotnet build -c Release

# 运行
dotnet run

# 发布
dotnet publish -c Release -o output
```

## 键盘快捷键

| 快捷键 | 功能 |
|--------|------|
| Ctrl+O | 打开左侧文件 |
| Ctrl+Shift+O | 打开右侧文件 |
| Ctrl+R | 一键比对 |
| Ctrl+Shift+R | 清空内容 |
| Ctrl+D | 切换深色/浅色模式 |
| Esc | 关闭比对结果 |

## 技术栈

- **框架**: .NET 8.0 WPF
- **编辑器**: AvalonEdit（WPF 原生代码编辑器）
- **比对引擎**: DiffPlex（C# 差异比对库）
- **打包**: dotnet publish + GitHub Actions
