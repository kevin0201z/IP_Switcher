using System;
using System.Collections.Generic;
using IP_Switcher.Interfaces;
using IP_Switcher.Models;

namespace IP_Switcher.Display
{
    /// <summary>
    /// 控制台显示层实现
    /// </summary>
    public class ConsoleDisplayLayer : IDisplayLayer
    {
        /// <summary>
        /// 显示欢迎界面
        /// </summary>
        public void ShowWelcome()
        {
            Console.Clear();
            Console.WriteLine("========================================");
            Console.WriteLine("IP切换控制台程序 v1.0");
            Console.WriteLine("========================================");
            Console.WriteLine();
        }

        /// <summary>
        /// 显示网卡列表并让用户选择
        /// </summary>
        /// <param name="nicList">网卡列表</param>
        /// <param name="defaultNicName">默认网卡名称</param>
        /// <returns>选择的网卡</returns>
        public NicInfo SelectNetworkAdapter(List<NicInfo> nicList, string defaultNicName)
        {
            if (nicList.Count == 0)
            {
                Console.WriteLine("未找到可用的网络适配器");
                return null;
            }

            Console.WriteLine("可用网卡列表：");
            for (int i = 0; i < nicList.Count; i++)
            {
                NicInfo nic = nicList[i];
                string status = nic.Status == System.Net.NetworkInformation.OperationalStatus.Up ? "已连接" : "已断开";
                string mark = (!string.IsNullOrEmpty(defaultNicName) && string.Equals(nic.Name, defaultNicName, StringComparison.OrdinalIgnoreCase)) ? " (默认)" : "";
                Console.WriteLine($"{i + 1}. [{status}] {nic.Name} - {nic.Type}{mark}");
            }
            Console.WriteLine();

            while (true)
            {
                Console.Write("请输入要配置的网卡序号（回车选择默认网卡）：");
                string input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                {
                    // 用户按回车：如果存在默认网卡且在列表中则选择它
                    if (!string.IsNullOrEmpty(defaultNicName))
                    {
                        var found = nicList.Find(n => string.Equals(n.Name, defaultNicName, StringComparison.OrdinalIgnoreCase));
                        if (found != null)
                        {
                            Console.WriteLine($"已选择网卡：{found.Name}");
                            return found;
                        }
                    }
                    Console.WriteLine("未设置默认网卡或默认网卡不在列表中，请输入序号。");
                    Console.WriteLine();
                    continue;
                }

                if (int.TryParse(input.Trim(), out int index) && index >= 1 && index <= nicList.Count)
                {
                    var selected = nicList[index - 1];
                    Console.WriteLine($"已选择网卡：{selected.Name}");
                    return selected;
                }
                else
                {
                    Console.WriteLine("无效的网卡选择，请重新输入！");
                }
            }
        }

        /// <summary>
        /// 显示网络配置列表并让用户选择
        /// </summary>
        /// <param name="configs">网络配置列表</param>
        /// <returns>选择的网络配置</returns>
        public NetworkConfig SelectNetworkConfig(List<NetworkConfig> configs)
        {
            Console.WriteLine("\n可用配置方案：");
            
            // 添加DHCP作为独立选项
            Console.WriteLine("1. DHCP自动获取");
            
            // 显示配置文件中的配置
            for (int i = 0; i < configs.Count; i++)
            {
                Console.WriteLine($"{i + 2}. {configs[i].Name}");
            }
            Console.WriteLine();

            while (true)
            {
                Console.Write("请输入要应用的配置序号：");
                string input = Console.ReadLine();

                if (int.TryParse(input, out int index))
                {
                    if (index == 1)
                    {
                        // 创建DHCP配置对象
                        return new NetworkConfig
                        {
                            Name = "DHCP自动获取",
                            NicName = "",
                            IPAddress = "",
                            SubnetMask = "",
                            DefaultGateway = "",
                            DnsServers = new List<string>()
                        };
                    }
                    else if (index >= 2 && index <= configs.Count + 1)
                    {
                        return configs[index - 2];
                    }
                }
                Console.WriteLine("无效的配置选择，请重新输入！");
            }
        }

        /// <summary>
        /// 显示当前网络配置
        /// </summary>
        /// <param name="nicName">网卡名称</param>
        /// <param name="config">网络配置</param>
        public void ShowCurrentConfig(string nicName, NetworkConfig config)
        {
            Console.WriteLine($"\n{nicName} 当前网络配置：");
            if (config == null)
            {
                Console.WriteLine("无法获取当前配置");
            }
            else
            {
                Console.WriteLine($"IP地址：{config.IPAddress}");
                Console.WriteLine($"子网掩码：{config.SubnetMask}");
                Console.WriteLine($"默认网关：{config.DefaultGateway}");
                Console.WriteLine($"DNS服务器：{string.Join(", ", config.DnsServers)}");
            }
            Console.WriteLine();
        }

        /// <summary>
        /// 显示配置应用进度和结果
        /// </summary>
        /// <param name="nicName">网卡名称</param>
        /// <param name="config">网络配置</param>
        /// <returns>用户是否确认应用配置</returns>
        public bool ShowConfigApplication(string nicName, NetworkConfig config)
        {
            Console.WriteLine($"正在将网卡 {nicName} 的配置切换为 {config.Name}...");
            Console.WriteLine($"目标IP：{config.IPAddress}");
            Console.WriteLine($"子网掩码：{config.SubnetMask}");
            Console.WriteLine($"默认网关：{config.DefaultGateway}");
            Console.WriteLine($"DNS服务器：{string.Join(", ", config.DnsServers)}");
            Console.WriteLine();

            Console.Write("确定要继续吗？(Y/N)：");
            string input = Console.ReadLine();
            if (input.ToUpper() != "Y")
            {
                Console.WriteLine("操作已取消");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 显示配置比较和确认界面
        /// </summary>
        /// <param name="selectedNic">选择的网卡</param>
        /// <param name="currentConfig">当前配置</param>
        /// <param name="targetConfig">目标配置</param>
        /// <returns>用户选择结果：true=确认，false=取消</returns>
        public bool ShowConfigComparison(NicInfo selectedNic, NetworkConfig currentConfig, NetworkConfig targetConfig)
        {
            Console.WriteLine($"\n当前网卡：{selectedNic.Name}");
            Console.WriteLine("当前配置：");
            DisplayConfigDetails(currentConfig);

            Console.WriteLine($"\n目标配置：{targetConfig.Name}");
            DisplayConfigDetails(targetConfig);

            Console.Write("\n确认要将当前配置切换为目标配置吗？(Y=确认  N=取消)：");
            string input = Console.ReadLine();
            return input.ToUpper() == "Y";
        }

        /// <summary>
        /// 显示配置应用结果
        /// </summary>
        /// <param name="success">是否成功</param>
        /// <param name="nicName">网卡名称</param>
        /// <param name="configName">配置名称</param>
        public void ShowApplicationResult(bool success, string nicName, string configName)
        {
            if (success)
            {
                Console.WriteLine($"\n成功将网卡 {nicName} 的配置切换为 {configName}！");
            }
            else
            {
                Console.WriteLine($"\n将网卡 {nicName} 的配置切换为 {configName} 失败！");
            }
        }

        /// <summary>
        /// 显示错误信息
        /// </summary>
        /// <param name="message">错误信息</param>
        public void ShowError(string message)
        {
            Console.WriteLine($"\n错误：{message}");
        }

        /// <summary>
        /// 显示信息
        /// </summary>
        /// <param name="message">信息内容</param>
        public void ShowInfo(string message)
        {
            Console.WriteLine($"\n{message}");
        }

        /// <summary>
        /// 等待用户按键
        /// </summary>
        public void WaitForKeyPress()
        {
            Console.WriteLine("\n按任意键退出程序...");
            Console.ReadKey();
        }

        /// <summary>
        /// 显示配置详情
        /// </summary>
        /// <param name="config">配置对象</param>
        private void DisplayConfigDetails(NetworkConfig config)
        {
            Console.WriteLine($"  IP地址：{config.IPAddress}");
            Console.WriteLine($"  子网掩码：{config.SubnetMask}");
            Console.WriteLine($"  默认网关：{config.DefaultGateway}");
            Console.WriteLine($"  DNS服务器：{string.Join(", ", config.DnsServers)}");
        }
    }
}