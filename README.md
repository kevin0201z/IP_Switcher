# IP_Switcher

## 项目概述

IP_Switcher是一款基于.NET Framework 4.8开发的网络IP配置切换工具，支持快速切换不同网卡的IP配置，包括DHCP自动获取和静态IP配置。

## 功能特性

- 🖥️ **直观的WPF界面**：简洁易用的图形界面，支持实时显示当前网络配置
- 🔄 **快速配置切换**：一键切换不同网络配置方案
- 📋 **多种配置管理**：支持添加、编辑、删除网络配置
- 🎯 **DHCP支持**：内置不可修改的DHCP自动获取配置项
- 💾 **配置持久化**：配置自动保存到本地文件
- 📍 **网卡记忆**：自动保存上次使用的网卡，下次启动时自动选中
- 📊 **实时配置显示**：显示当前网卡的详细网络配置信息

## 技术栈

- .NET Framework 4.8
- WPF (Windows Presentation Foundation)
- C#
- .NET Standard 2.0

## 项目结构

```text
IP_Switcher/
├── IP_Switcher/               # 核心业务逻辑库
│   ├── Models/                # 数据模型
│   ├── Interfaces/            # 接口定义
│   ├── ConfigManager.cs       # 配置管理
│   ├── NetworkManager.cs      # 网络配置管理
│   └── Logger.cs              # 日志管理
├── IP_Switcher_GUI/           # Windows Forms 界面
├── IP_Switcher_WPF/           # WPF 界面
│   ├── MainWindow.xaml        # 主界面
│   ├── EditConfigWindow.xaml  # 配置编辑界面
│   └── MainWindow.xaml.cs     # 主界面逻辑
├── Tests/                     # 单元测试
└── config.json                # 配置文件
```

## 安装说明

1. 确保安装了.NET Framework 4.8运行时
2. 从GitHub仓库克隆或下载项目
3. 使用Visual Studio 2019或更高版本打开解决方案
4. 构建解决方案生成可执行文件
5. 运行 `IP_Switcher_WPF/bin/Release/IP_Switcher_WPF.exe`

## 使用方法

### 1. 选择网卡

- 在下拉菜单中选择要配置的网络适配器
- 系统会自动显示当前网卡的网络配置

### 2. 管理配置

- **添加配置**：点击"添加"按钮，填写网络配置信息
- **编辑配置**：选中配置项，点击"编辑"按钮修改配置
- **删除配置**：选中配置项，点击"删除"按钮移除配置
- **DHCP配置**：内置的DHCP配置项不可修改或删除

### 3. 应用配置

- 选择要应用的网络配置
- 点击"应用"按钮，确认后系统会应用所选配置
- 应用成功后，当前配置显示会更新

## 配置文件

配置信息保存在项目根目录的 `config.json` 文件中，格式如下：

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

## 日志记录

系统会在 `Logs` 目录下生成日志文件，记录应用程序运行状态和错误信息，便于调试和监控。

## 注意事项

- 应用网络配置需要管理员权限
- 切换网络配置可能会导致网络短暂中断
- 确保在安全的网络环境下使用

## 开发说明

### 构建项目

```bash
dotnet build IP_Switcher.sln /p:Configuration=Release
```

### 运行测试

```bash
dotnet test IP_Switcher.sln
```

**IP_Switcher** - 让网络配置切换更简单！
