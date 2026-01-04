using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using IP_Switcher.Models;

namespace IP_Switcher
{
    /// <summary>
    /// 网络管理类
    /// </summary>
    public class NetworkManager : INetworkManager
    {
        /// <summary>
        /// 错误事件，用于通知调用者发生的错误
        /// </summary>
        public event EventHandler<NetworkErrorEventArgs> ErrorOccurred;

        /// <summary>
        /// 触发错误事件
        /// </summary>
        /// <param name="errorMessage">错误消息</param>
        /// <param name="exception">异常对象</param>
        protected virtual void OnErrorOccurred(string errorMessage, Exception exception = null)
        {
            ErrorOccurred?.Invoke(this, new NetworkErrorEventArgs(errorMessage, exception));
            Console.WriteLine($"错误: {errorMessage}");
            if (exception != null)
            {
                Console.WriteLine($"异常: {exception.Message}");
                Console.WriteLine($"堆栈跟踪: {exception.StackTrace}");
            }
        }

        /// <summary>
        /// 获取所有可用网卡信息
        /// </summary>
        /// <returns>网卡信息列表</returns>
        public List<NicInfo> GetAllNetworkAdapters()
        {
            List<NicInfo> nicList = new List<NicInfo>();
            
            try
            {
                // 获取所有网络接口
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
                
                foreach (var ni in interfaces)
                {
                    // 过滤掉回环接口和禁用的接口
                    if (ni.NetworkInterfaceType != NetworkInterfaceType.Loopback && 
                        ni.OperationalStatus != OperationalStatus.Down)
                    {
                        NicInfo nicInfo = new NicInfo(ni);
                        nicList.Add(nicInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                OnErrorOccurred("获取网卡信息失败", ex);
            }
            
            return nicList;
        }

        /// <summary>
        /// 获取指定名称的网卡
        /// </summary>
        /// <param name="nicName">网卡名称</param>
        /// <returns>网卡信息</returns>
        public NicInfo GetNetworkAdapterByName(string nicName)
        {
            try
            {
                NetworkInterface ni = NetworkInterface.GetAllNetworkInterfaces()
                    .FirstOrDefault(n => n.Name == nicName);
                
                if (ni != null)
                {
                    return new NicInfo(ni);
                }
            }
            catch (Exception ex)
            {
                OnErrorOccurred("获取指定网卡信息失败", ex);
            }
            
            return null;
        }

        /// <summary>
        /// 获取网卡当前IP配置
        /// </summary>
        /// <param name="nicName">网卡名称</param>
        /// <returns>当前IP配置信息</returns>
        public NetworkConfig GetCurrentIpConfig(string nicName)
        {
            NetworkConfig config = new NetworkConfig();
            
            try
            {
                NetworkInterface ni = NetworkInterface.GetAllNetworkInterfaces()
                    .FirstOrDefault(n => n.Name == nicName);
                
                if (ni != null)
                {
                    config.NicName = ni.Name;
                    
                    // 获取IP属性
                    IPInterfaceProperties ipProps = ni.GetIPProperties();
                    
                    // 获取单播地址（IP地址）
                    var unicastAddress = ipProps.UnicastAddresses.FirstOrDefault(
                        addr => addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                    
                    if (unicastAddress != null)
                    {
                        config.IPAddress = unicastAddress.Address.ToString();
                        config.SubnetMask = unicastAddress.IPv4Mask.ToString();
                    }
                    
                    // 获取默认网关
                    var gateway = ipProps.GatewayAddresses.FirstOrDefault(
                        addr => addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                    
                    if (gateway != null)
                    {
                        config.DefaultGateway = gateway.Address.ToString();
                    }
                    
                    // 获取DNS服务器
                    foreach (var dns in ipProps.DnsAddresses)
                    {
                        if (dns.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            config.DnsServers.Add(dns.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                OnErrorOccurred("获取当前IP配置失败", ex);
            }
            
            return config;
        }

        /// <summary>
        /// 检查当前进程是否以管理员权限运行
        /// </summary>
        /// <returns>是否为管理员权限</returns>
        private bool IsRunningAsAdmin()
        {
            try
            {
                System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
                System.Security.Principal.WindowsPrincipal principal = new System.Security.Principal.WindowsPrincipal(identity);
                return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// 验证IP地址格式是否正确
        /// </summary>
        /// <param name="ipAddress">IP地址</param>
        /// <returns>是否为有效的IP地址</returns>
        public bool IsValidIpAddress(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
                return false;
            
            return System.Net.IPAddress.TryParse(ipAddress, out _);
        }

        /// <summary>
        /// 验证网络配置是否有效
        /// </summary>
        /// <param name="config">网络配置</param>
        /// <returns>是否为有效的网络配置</returns>
        public bool IsValidNetworkConfig(NetworkConfig config)
        {
            if (config == null)
                return false;
            
            // 如果是DHCP配置（IP 和 子网掩码都为空），无需验证IP地址等参数
            if (string.IsNullOrEmpty(config.IPAddress) && string.IsNullOrEmpty(config.SubnetMask))
                return true;
            
            // 验证IP地址格式
            if (!IsValidIpAddress(config.IPAddress))
                return false;
            
            // 验证子网掩码格式
            if (!IsValidIpAddress(config.SubnetMask))
                return false;
            
            // 验证默认网关格式（如果填写）
            if (!string.IsNullOrEmpty(config.DefaultGateway) && !IsValidIpAddress(config.DefaultGateway))
                return false;
            
            // 验证DNS服务器格式（如果填写）
            if (config.DnsServers != null)
            {
                foreach (string dns in config.DnsServers)
                {
                    if (!string.IsNullOrEmpty(dns) && !IsValidIpAddress(dns))
                        return false;
                }
            }
            
            return true;
        }

        /// <summary>
        /// 设置网卡IP配置
        /// </summary>
        /// <param name="nicName">网卡名称</param>
        /// <param name="config">网络配置</param>
        /// <returns>是否设置成功</returns>
        public bool SetIpConfig(string nicName, NetworkConfig config)
        {
            try
            {
                // 检查是否以管理员权限运行
                if (!IsRunningAsAdmin())
                {
                    OnErrorOccurred("设置IP配置需要管理员权限");
                    return false;
                }

                // 验证网卡名称是否存在，防止命令注入和误操作
                var nic = NetworkInterface.GetAllNetworkInterfaces()
                    .FirstOrDefault(n => n.Name == nicName);
                if (nic == null)
                {
                    OnErrorOccurred($"未找到网卡：{nicName}");
                    return false;
                }
                
                if (string.IsNullOrEmpty(config.IPAddress) || string.IsNullOrEmpty(config.SubnetMask))
                {
                    // 设置为DHCP自动获取
                    return SetDhcpConfig(nicName);
                }
                else
                {
                    // 验证输入参数格式
                    if (!IsValidNetworkConfig(config))
                    {
                        Console.WriteLine("无效的网络配置格式");
                        return false;
                    }
                    
                    // 设置为静态IP
                    // 构建设置IP地址的命令，根据是否有网关条件性添加
                    string setAddressCommand = $"netsh interface ipv4 set address name=\"{nicName}\" source=static addr={config.IPAddress} mask={config.SubnetMask}";
                    if (!string.IsNullOrEmpty(config.DefaultGateway))
                    {
                        setAddressCommand += $" gateway={config.DefaultGateway} gwmetric=1";
                    }
                    var addrResult = ExecuteCommand(setAddressCommand);
                    if (addrResult.exitCode != 0)
                    {
                        OnErrorOccurred($"设置静态IP失败: {addrResult.error}");
                        return false;
                    }
                    
                    // 设置DNS服务器
                    // 始终清空现有DNS配置（尝试使用 ipv4 命令集）
                    ExecuteCommand($"netsh interface ipv4 delete dns name=\"{nicName}\" addr=all");

                    if (config.DnsServers != null && config.DnsServers.Count > 0)
                    {
                        // 添加主DNS
                        var dnsResult = ExecuteCommand($"netsh interface ipv4 add dns name=\"{nicName}\" addr={config.DnsServers[0]} index=1");
                        if (dnsResult.exitCode != 0)
                        {
                            OnErrorOccurred($"设置主DNS失败: {dnsResult.error}");
                            // 继续尝试但记录错误
                        }

                        // 添加备用DNS（如果有）
                        for (int i = 1; i < config.DnsServers.Count; i++)
                        {
                            ExecuteCommand($"netsh interface ipv4 add dns name=\"{nicName}\" addr={config.DnsServers[i]} index={i + 1}");
                        }
                    }
                    
                    // 验证配置是否成功
                    NetworkConfig currentConfig = GetCurrentIpConfig(nicName);
                    if (currentConfig != null &&
                        string.Equals(currentConfig.IPAddress?.Trim(), config.IPAddress?.Trim(), StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(currentConfig.SubnetMask?.Trim(), config.SubnetMask?.Trim(), StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(currentConfig.DefaultGateway?.Trim(), config.DefaultGateway?.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("配置验证失败，当前配置与期望配置不符");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"设置IP配置失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 设置网卡为DHCP自动获取IP
        /// </summary>
        /// <param name="nicName">网卡名称</param>
        /// <returns>是否设置成功</returns>
        public bool SetDhcpConfig(string nicName)
        {
            try
            {
                // 检查是否以管理员权限运行
                if (!IsRunningAsAdmin())
                {
                    OnErrorOccurred("设置DHCP配置需要管理员权限");
                    return false;
                }
                // 验证网卡名称是否存在
                var nic = NetworkInterface.GetAllNetworkInterfaces()
                    .FirstOrDefault(n => n.Name == nicName);
                if (nic == null)
                {
                    OnErrorOccurred($"未找到网卡：{nicName}");
                    return false;
                }
                
                // 设置IP地址为DHCP自动获取
                ExecuteCommand($"netsh interface ipv4 set address name=\"{nicName}\" source=dhcp");
                
                // 设置DNS为DHCP自动获取
                ExecuteCommand($"netsh interface ipv4 set dns name=\"{nicName}\" source=dhcp");
                
                // DHCP配置后需要时间从服务器获取IP，添加延迟后重试验证
                const int MAX_RETRIES = 10; // 最多重试10次
                const int INITIAL_DELAY_MS = 500; // 初始延迟500毫秒
                const int MAX_DELAY_MS = 3000; // 最大延迟3000毫秒
                
                for (int i = 0; i < MAX_RETRIES; i++)
                {
                    // 计算当前重试的延迟时间（指数退避策略）
                    int delayMs = Math.Min(INITIAL_DELAY_MS * (int)Math.Pow(2, i), MAX_DELAY_MS);
                    System.Threading.Thread.Sleep(delayMs);
                    
                    NetworkConfig currentConfig = GetCurrentIpConfig(nicName);
                    if (!string.IsNullOrEmpty(currentConfig.IPAddress))
                    {
                        Console.WriteLine($"DHCP配置成功，获取到IP地址: {currentConfig.IPAddress}");
                        return true;
                    }
                    
                    Console.WriteLine($"DHCP配置重试 {i+1}/{MAX_RETRIES}，等待 {delayMs} 毫秒后再次检查");
                }
                
                OnErrorOccurred("DHCP配置验证失败，未能在规定时间内获取到IP地址");
                return false;
            }
            catch (Exception ex)
            {
                OnErrorOccurred("设置DHCP配置失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 执行命令行命令
        /// </summary>
        /// <param name="command">命令</param>
        /// <returns>命令执行结果</returns>
        private (string output, int exitCode, string error) ExecuteCommand(string command)
        {
            using (System.Diagnostics.Process process = new System.Diagnostics.Process())
            {
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = $"/c {command}";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;
                
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                int exitCode = process.ExitCode;
                
                if (!string.IsNullOrEmpty(error))
                {
                    Console.WriteLine($"命令执行错误: {error}");
                }
                
                return (output, exitCode, error);
            }
        }
    }
}