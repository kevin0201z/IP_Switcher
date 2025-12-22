using System;
using System.Collections.Generic;
using IP_Switcher.Models;

namespace IP_Switcher
{
    /// <summary>
    /// 配置管理器接口，定义了配置文件相关的操作
    /// </summary>
    public interface IConfigManager
    {
        /// <summary>
        /// 加载默认配置文件
        /// </summary>
        /// <returns>网络配置列表</returns>
        List<NetworkConfig> LoadConfig();

        /// <summary>
        /// 加载指定路径的配置文件
        /// </summary>
        /// <param name="configPath">配置文件路径</param>
        /// <returns>网络配置列表</returns>
        List<NetworkConfig> LoadConfig(string configPath);

        /// <summary>
        /// 创建默认配置文件
        /// </summary>
        /// <param name="configPath">配置文件路径</param>
        void CreateDefaultConfig(string configPath = null);

        /// <summary>
        /// 保存配置到文件
        /// </summary>
        /// <param name="configs">网络配置列表</param>
        /// <param name="configPath">配置文件路径</param>
        void SaveConfig(List<NetworkConfig> configs, string configPath = null);

        /// <summary>
        /// 获取上次使用的网卡名称
        /// </summary>
        /// <returns>网卡名称</returns>
        string GetLastNic();

        /// <summary>
        /// 设置上次使用的网卡名称
        /// </summary>
        /// <param name="nicName">网卡名称</param>
        void SetLastNic(string nicName);

        /// <summary>
        /// 根据名称获取配置
        /// </summary>
        /// <param name="configs">配置列表</param>
        /// <param name="configName">配置名称</param>
        /// <returns>网络配置</returns>
        NetworkConfig GetConfigByName(List<NetworkConfig> configs, string configName);
    }
}