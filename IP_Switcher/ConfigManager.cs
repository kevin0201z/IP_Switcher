using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using IP_Switcher.Models;

namespace IP_Switcher
{
    /// <summary>
    /// 配置管理类
    /// </summary>
    public class ConfigManager : IConfigManager
    {
        private readonly string defaultConfigPath = "config.json";
        
        /// <summary>
        /// 加载默认配置文件
        /// </summary>
        /// <returns>网络配置列表</returns>
        public List<NetworkConfig> LoadConfig()
        {
            return LoadConfig(defaultConfigPath);
        }

        /// <summary>
        /// 加载指定路径的配置文件
        /// </summary>
        /// <param name="configPath">配置文件路径</param>
        /// <returns>网络配置列表</returns>
        public List<NetworkConfig> LoadConfig(string configPath)
        {
            List<NetworkConfig> configs = new List<NetworkConfig>();
            
            try
            {
                if (File.Exists(configPath))
                {
                    string jsonString = File.ReadAllText(configPath);
                    ConfigRoot root = JsonSerializer.Deserialize<ConfigRoot>(jsonString);
                    configs = root?.Configurations ?? new List<NetworkConfig>();
                }
                else
                {
                    Console.WriteLine($"配置文件不存在: {configPath}");
                    // 创建默认配置文件
                    CreateDefaultConfig(configPath);
                    configs = LoadConfig(configPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载配置文件失败: {ex.Message}");
            }
            
            return configs;
        }

        /// <summary>
        /// 读取配置文件中的 LastNic 字段（上次选择的网卡名称）。
        /// 返回 null 表示未设置或读取失败。
        /// </summary>
        public string GetLastNic()
        {
            try
            {
                if (File.Exists(defaultConfigPath))
                {
                    string jsonString = File.ReadAllText(defaultConfigPath);
                    ConfigRoot root = JsonSerializer.Deserialize<ConfigRoot>(jsonString);
                    return string.IsNullOrWhiteSpace(root?.LastNic) ? null : root.LastNic;
                }
            }
            catch (Exception)
            {
                // 忽略，返回 null
            }
            return null;
        }

        /// <summary>
        /// 将上次选择的网卡名称写入配置文件的 LastNic 字段（若文件存在则更新，否则创建）。
        /// </summary>
        public void SetLastNic(string nicName)
        {
            try
            {
                ConfigRoot root = new ConfigRoot();
                if (File.Exists(defaultConfigPath))
                {
                    string jsonString = File.ReadAllText(defaultConfigPath);
                    root = JsonSerializer.Deserialize<ConfigRoot>(jsonString) ?? new ConfigRoot();
                }

                root.LastNic = nicName ?? string.Empty;

                string updatedJson = JsonSerializer.Serialize(root, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(defaultConfigPath, updatedJson);
            }
            catch (Exception)
            {
                // 忽略写入失败
            }
        }

        /// <summary>
        /// 创建默认配置文件
        /// </summary>
        /// <param name="configPath">配置文件路径</param>
        public void CreateDefaultConfig(string configPath = null)
        {
            if (string.IsNullOrEmpty(configPath))
            {
                configPath = defaultConfigPath;
            }
            
            try
            {
                // 创建默认配置
                List<NetworkConfig> defaultConfigs = new List<NetworkConfig>
                {
                    new NetworkConfig
                    {
                        Name = "办公室网络",
                        NicName = "以太网",
                        IPAddress = "192.168.1.100",
                        SubnetMask = "255.255.255.0",
                        DefaultGateway = "192.168.1.1",
                        DnsServers = new List<string> { "8.8.8.8", "8.8.4.4" }
                    },
                    new NetworkConfig
                    {
                        Name = "家庭网络",
                        NicName = "以太网",
                        IPAddress = "10.0.0.5",
                        SubnetMask = "255.255.255.0",
                        DefaultGateway = "10.0.0.1",
                        DnsServers = new List<string> { "114.114.114.114" }
                    }
                };
                
                ConfigRoot root = new ConfigRoot { Configurations = defaultConfigs };
                
                string jsonString = JsonSerializer.Serialize(root, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(configPath, jsonString);
                
                Console.WriteLine($"已创建默认配置文件: {configPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"创建默认配置文件失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 保存配置到文件
        /// </summary>
        /// <param name="configs">网络配置列表</param>
        /// <param name="configPath">配置文件路径</param>
        public void SaveConfig(List<NetworkConfig> configs, string configPath = null)
        {
            if (string.IsNullOrEmpty(configPath))
            {
                configPath = defaultConfigPath;
            }
            
            try
            {
                // 如果文件存在，尝试保留已有的 LastNic 字段
                ConfigRoot root = new ConfigRoot { Configurations = configs };
                if (File.Exists(configPath))
                {
                    try
                    {
                        string existingJson = File.ReadAllText(configPath);
                        ConfigRoot existing = JsonSerializer.Deserialize<ConfigRoot>(existingJson);
                        if (existing != null)
                        {
                            root.LastNic = existing.LastNic;
                        }
                    }
                    catch
                    {
                        // 忽略读取错误，继续保存新的根对象
                    }
                }
                
                string jsonString = JsonSerializer.Serialize(root, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(configPath, jsonString);
                
                Console.WriteLine($"配置已保存到文件: {configPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存配置文件失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 根据名称获取配置
        /// </summary>
        /// <param name="configs">配置列表</param>
        /// <param name="configName">配置名称</param>
        /// <returns>网络配置</returns>
        public NetworkConfig GetConfigByName(List<NetworkConfig> configs, string configName)
        {
            return configs.FirstOrDefault(c => c.Name == configName);
        }
    }

    /// <summary>
    /// 配置文件根对象
    /// </summary>
    internal class ConfigRoot
    {
        public List<NetworkConfig> Configurations { get; set; }

        public string LastNic { get; set; }
        
        public ConfigRoot()
        {
            Configurations = new List<NetworkConfig>();
            LastNic = null;
        }
    }

    /// <summary>
    /// 网络配置数据契约
    /// </summary>
    public class NetworkConfigDataContract
    {
        public string Name { get; set; }
        
        public string NicName { get; set; }
        
        public string IPAddress { get; set; }
        
        public string SubnetMask { get; set; }
        
        public string DefaultGateway { get; set; }
        
        public List<string> DnsServers { get; set; }
        
        public NetworkConfigDataContract()
        {
            DnsServers = new List<string>();
        }
    }
}