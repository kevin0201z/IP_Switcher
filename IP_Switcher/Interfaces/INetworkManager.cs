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
        /// 错误事件，用于通知调用者发生的错误
        /// </summary>
        event EventHandler<NetworkErrorEventArgs> ErrorOccurred;

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

        /// <summary>
        /// 设置网卡为DHCP自动获取IP
        /// </summary>
        /// <param name="nicName">网卡名称</param>
        /// <returns>是否设置成功</returns>
        bool SetDhcpConfig(string nicName);

        /// <summary>
        /// 验证IP地址格式是否正确
        /// </summary>
        /// <param name="ipAddress">IP地址</param>
        /// <returns>是否为有效的IP地址</returns>
        bool IsValidIpAddress(string ipAddress);

        /// <summary>
        /// 验证网络配置是否有效
        /// </summary>
        /// <param name="config">网络配置</param>
        /// <returns>是否为有效的网络配置</returns>
        bool IsValidNetworkConfig(NetworkConfig config);
    }

    /// <summary>
    /// 网络错误事件参数
    /// </summary>
    public class NetworkErrorEventArgs : EventArgs
    {
        /// <summary>
        /// 错误消息
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 异常对象
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="errorMessage">错误消息</param>
        /// <param name="exception">异常对象</param>
        public NetworkErrorEventArgs(string errorMessage, Exception exception = null)
        {
            ErrorMessage = errorMessage;
            Exception = exception;
        }
    }
}