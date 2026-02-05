# IP_Switcher

## 项目概述

IP_Switcher 是一款基于 .NET Framework 4.8 的网络 IP 配置切换工具，支持快速在不同网络配置间切换（包括 DHCP 和静态 IP）。仓库包含多种界面实现：WPF、WinForms（GUI）以及控制台实现，核心逻辑位于 `IP_Switcher` 项目中。

## 主要特性

- 直观的图形界面（WPF / WinForms）和控制台工具
- 一键切换网络配置（DHCP / 静态 IP）
- 配置管理：添加、编辑、删除配置项
- 配置持久化到 `config.json`
- 日志记录到 `Logs` 目录，便于排查问题

## 技术栈

- .NET Framework 4.8
- WPF / WinForms
- C# (.NET Standard 2.0 用于共享库)

## 仓库结构（精简）

```text
IP_Switcher.sln
config.json
IP_Switcher/               # 核心库与控制台实现
IP_Switcher_GUI/           # Windows Forms 界面
IP_Switcher_WPF/           # WPF 界面
Tests/                     # 单元测试项目
Logs/                      # 运行时日志
```

## 安装与构建

1. 安装 .NET Framework 4.8 运行时（和 Visual Studio 用于开发）。
2. 克隆或下载仓库并在 Visual Studio 中打开 `IP_Switcher.sln`。
3. 使用 Visual Studio 构建解决方案，或在命令行中运行：

```powershell
dotnet build IP_Switcher.sln /p:Configuration=Release
```

构建后可执行文件位于对应子项目的 `bin/Release` 目录，例如：

- `IP_Switcher_WPF/bin/Release/IP_Switcher_WPF.exe`（WPF 界面）
- `IP_Switcher_GUI/bin/Release/IP_Switcher_GUI.exe`（WinForms 界面）

注意：应用网络配置需要以管理员权限运行。

## 使用说明（快速）

1. 启动 WPF 或 WinForms 界面程序。
2. 在界面中选择要操作的网卡（下拉菜单），界面会显示当前网络信息。
3. 新增或编辑配置后，选择配置并点击“应用”以切换网络设置。

## 配置文件

根目录下的 `config.json` 存储配置信息，例如：

```json
{
  "LastNic": "以太网",
  "Configs": [
    {
      "Name": "办公室配置",
      "NicName": "以太网",
      "IPAddress": "192.168.1.100",
      "SubnetMask": "255.255.255.0",
      "DefaultGateway": "192.168.1.1",
      "DnsServers": ["8.8.8.8", "114.114.114.114"]
    }
  ]
}
```

## 日志

运行时日志输出到 `Logs` 目录，帮助定位运行时错误和操作记录。

## 开发与测试

- 在 Visual Studio 中打开 `IP_Switcher.sln` 并运行/调试对应项目。
- 使用命令行运行单元测试：

```powershell
dotnet test IP_Switcher.sln
```

## 注意事项

- 更改网络设置需要管理员权限，执行前请保存重要工作。
- 切换网络配置可能导致短暂断网。

---
