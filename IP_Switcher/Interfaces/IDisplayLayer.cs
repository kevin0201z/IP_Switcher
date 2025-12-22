using System;
using System.Collections.Generic;
using IP_Switcher.Models;

namespace IP_Switcher.Interfaces
{
    /// <summary>
    /// 显示层接口，定义了所有与显示相关的操作
    /// </summary>
    public interface IDisplayLayer
    {
        /// <summary>
        /// 显示欢迎界面
        /// </summary>
        void ShowWelcome();

        /// <summary>
        /// 显示网卡列表并让用户选择
        /// </summary>
        /// <param name="nicList">网卡列表</param>
        /// <param name="defaultNicName">默认网卡名称</param>
        /// <returns>选择的网卡</returns>
        NicInfo SelectNetworkAdapter(List<NicInfo> nicList, string defaultNicName);

        /// <summary>
        /// 显示网络配置列表并让用户选择
        /// </summary>
        /// <param name="configs">网络配置列表</param>
        /// <returns>选择的网络配置</returns>
        NetworkConfig SelectNetworkConfig(List<NetworkConfig> configs);

        /// <summary>
        /// 显示当前网络配置
        /// </summary>
        /// <param name="nicName">网卡名称</param>
        /// <param name="config">网络配置</param>
        void ShowCurrentConfig(string nicName, NetworkConfig config);

        /// <summary>
        /// 显示配置应用进度和结果
        /// </summary>
        /// <param name="nicName">网卡名称</param>
        /// <param name="config">网络配置</param>
        /// <returns>用户是否确认应用配置</returns>
        bool ShowConfigApplication(string nicName, NetworkConfig config);

        /// <summary>
        /// 显示配置比较和确认界面
        /// </summary>
        /// <param name="selectedNic">选择的网卡</param>
        /// <param name="currentConfig">当前配置</param>
        /// <param name="targetConfig">目标配置</param>
        /// <returns>用户选择结果：true=确认，false=取消</returns>
        bool ShowConfigComparison(NicInfo selectedNic, NetworkConfig currentConfig, NetworkConfig targetConfig);

        /// <summary>
        /// 显示配置应用结果
        /// </summary>
        /// <param name="success">是否成功</param>
        /// <param name="nicName">网卡名称</param>
        /// <param name="configName">配置名称</param>
        void ShowApplicationResult(bool success, string nicName, string configName);

        /// <summary>
        /// 显示错误信息
        /// </summary>
        /// <param name="message">错误信息</param>
        void ShowError(string message);

        /// <summary>
        /// 显示信息
        /// </summary>
        /// <param name="message">信息内容</param>
        void ShowInfo(string message);

        /// <summary>
        /// 等待用户按键
        /// </summary>
        void WaitForKeyPress();
    }
}