using System;
using System.Collections.Generic;
using IP_Switcher.Models;

namespace IP_Switcher
{
    /// <summary>
    /// 网络管理器接口，定义了网络相关的操作
    /// </summary>
    public interface INetworkManager
    {
        /// <summary>
        /// 获取所有可用网卡信息
        /// </summary>
        /// <returns>网卡信息列表</returns>
        List<NicInfo> GetAllNetworkAdapters();

        /// <summary>
        /// 获取指定名称的网卡
        /// </summary>
        /// <param name="nicName">网卡名称</param>
        /// <returns>网卡信息</returns>
        NicInfo GetNetworkAdapterByName(string nicName);

        /// <summary>
        /// 获取网卡当前IP配置
        /// </summary>
        /// <param name="nicName">网卡名称</param>
        /// <returns>当前IP配置信息</returns>
        NetworkConfig GetCurrentIpConfig(string nicName);

        /// <summary>
        /// 设置网卡IP配置
        /// </summary>
        /// <param name="nicName">网卡名称</param>
        /// <param name="config">网络配置</param>
        /// <returns>是否设置成功</returns>
        bool SetIpConfig(string nicName, NetworkConfig config);
    }
}