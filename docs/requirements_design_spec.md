# IP_Switcher 需求设计说明书

文档版本：v1.0

编制日期：2026-03-06

项目名称：IP_Switcher

## 1. 文档目的

本文档基于当前仓库代码实现（`IP_Switcher.sln`）进行逆向分析，输出可用于开发、测试和交付的需求与设计说明。

## 2. 项目背景与目标

IP_Switcher 是一个 Windows 环境下的 IP 配置切换工具，目标是让用户在不同网络场景（办公室、家庭、测试环境）之间快速切换网卡配置，降低手工修改网络参数的成本与出错率。

## 3. 系统范围

- 核心库：`IP_Switcher`（配置管理、网络管理、日志、控制台交互）
- 图形界面：`IP_Switcher_WPF`（主交付界面）
- 图形界面：`IP_Switcher_GUI`（WinForms 版本）
- 测试工程：`Tests`

不在本期范围：

- 跨平台支持（当前依赖 `netsh` 和 Windows 管理员权限）
- IPv6 配置切换
- 远程集中配置下发

## 4. 用户角色与使用场景

- 普通运维/测试用户：按配置模板切换 IP
- 开发人员：维护配置模板、查看日志、排查失败原因

典型场景：

- 连接公司内网时使用静态 IP
- 切换到公共网络时恢复 DHCP 自动获取
- 切换后在倒计时窗口确认配置可用，避免误操作导致断网

## 5. 功能需求

### FR-001 网卡发现

系统应列出当前机器可用网卡（过滤 Loopback 和 Down 状态网卡），供用户选择。

### FR-002 当前配置读取

系统应在用户选中网卡后显示该网卡当前 IPv4 配置：IP、子网掩码、默认网关、DNS。

### FR-003 配置模板管理

系统应支持新增、编辑、删除网络配置模板。

约束：

- 配置名称不能为空
- 配置名称需唯一（同一配置集内）
- DHCP 内置模板不可编辑、不可删除

### FR-004 模板持久化

系统应将模板保存到根目录 `config.json`，并保存最近一次选中的网卡 `LastNic`。

### FR-005 IP 切换执行

系统应支持将模板应用到指定网卡：

- 当 IP/Mask 为空时，按 DHCP 模式切换
- 当 IP/Mask 有值时，按静态 IP 模式切换
- DNS 先清空后按序写入

### FR-006 管理员权限校验

系统应在执行网络配置前校验管理员权限；WPF 启动时需直接校验并阻断无权限运行。

### FR-007 切换确认与自动回滚（WPF）

WPF 版本在应用配置成功后，应弹出 10 秒倒计时确认框：

- 用户确认：保留新配置
- 用户取消或超时：自动恢复原配置

### FR-008 输入校验

系统应对 IP、子网掩码、网关、DNS 做格式校验（IPv4 字符串可解析）。

### FR-009 日志记录

系统应写日志到 `Logs/IPSwitcher_yyyyMMdd.log`，用于问题追踪和审计。

## 6. 非功能需求

### NFR-001 平台约束

- 操作系统：Windows
- 运行时：.NET Framework 4.8

### NFR-002 安全性

- 网卡名必须与系统网卡列表匹配，防止命令注入和误操作
- 关键操作需二次确认（WPF/WinForms）

### NFR-003 可用性

- WPF 启动采用异步加载网卡和配置，降低界面卡顿
- 失败场景给出可读提示信息

### NFR-004 可维护性

- 核心能力通过接口抽象（`INetworkManager`、`IConfigManager`、`ILogger`）
- 支持依赖注入和单元测试替身（Fake）

## 7. 总体设计

### 7.1 架构分层

- 表示层：WPF / WinForms / Console
- 业务协调层：`MainViewModel`、`ConsoleUI`
- 领域与服务层：`NetworkManager`、`ConfigManager`、`Logger`
- 数据层：`config.json`、日志文件

### 7.2 关键流程

流程 A：应用静态 IP 模板

1. 选择网卡与模板
2. 校验参数格式
3. 调用 `netsh interface ipv4 set address ... source=static ...`
4. 清空并重设 DNS
5. 回读当前配置进行一致性校验
6. WPF 弹出确认窗口，超时可回滚

流程 B：应用 DHCP 模板

1. 调用 `netsh interface ipv4 set address ... source=dhcp`
2. 调用 `netsh interface ipv4 set dns ... source=dhcp`
3. 采用指数退避重试（最多 10 次）验证是否拿到 IP

## 8. 数据设计

### 8.1 配置文件

路径：项目根目录 `config.json`

主要字段：

- `Configurations`: `NetworkConfig[]`
- `LastNic`: string

`NetworkConfig` 字段：

- `Name`
- `NicName`
- `IPAddress`
- `SubnetMask`
- `DefaultGateway`
- `DnsServers` (string[])

### 8.2 日志文件

- 目录：`Logs`
- 文件名：`IPSwitcher_yyyyMMdd.log`
- 保留策略：清理 7 天前旧日志

## 9. 接口与组件设计

### 9.1 INetworkManager

核心职责：网卡枚举、当前配置读取、IP 应用、DHCP 切换、参数校验。

### 9.2 IConfigManager

核心职责：配置文件加载/保存、默认配置创建、最后网卡读写。

### 9.3 ILogger

核心职责：统一日志输出接口。

## 10. 异常与边界处理

- 无管理员权限：拒绝执行并提示
- 网卡不存在：终止操作并记录错误
- 配置不合法：拒绝写入系统网络栈
- DHCP 获取失败：重试后失败并返回错误
- 写配置/日志失败：捕获异常，避免应用崩溃

## 11. 测试与质量现状（2026-03-06）

执行命令：`dotnet test IP_Switcher.sln`

结果：

- 总计 15
- 通过 13
- 失败 2

失败项：

- `LoggerTests.Info_ShouldWriteToLogFile`
- `LoggerTests.Warning_ShouldWriteToLogFile`

原因分析：`Logger.WriteLog` 当前仅写入 `Error` 级别（`if (level < LogLevel.Error) return;`），与测试对 `Info/Warning` 的期望不一致。

## 12. 已知风险与改进建议

- 风险 1：文档与实现字段名存在偏差风险（README 样例与代码使用字段需保持一致）
- 风险 2：仅支持 IPv4，IPv6 环境下能力不足
- 风险 3：WinForms 未实现 WPF 的倒计时自动回滚能力

建议：

- 修复日志等级策略与测试预期不一致问题
- 补充 NetworkManager 的集成测试（可通过抽象命令执行器实现可测性）
- 统一各前端对 DHCP 模板的表现和交互策略

## 13. 验收标准

- 可成功加载网卡并显示当前配置
- 可新增/编辑/删除模板并持久化到 `config.json`
- 可对指定网卡成功应用 DHCP 或静态模板
- 失败场景有明确提示且不崩溃
- WPF 版本支持切换后确认与自动回滚

## 14. 交付物

- 本文档：需求设计说明书（Word）
- 源文件：`docs/需求设计说明书.md`
- 生成文件：`docs/IP_Switcher_需求设计说明书_v1.0.docx`
