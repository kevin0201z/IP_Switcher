using System;
using System.Collections.Generic;
using System.Windows.Forms;
using IP_Switcher.Interfaces;
using IP_Switcher.Models;

namespace IP_Switcher.Display.WinForm
{
    /// <summary>
    /// WinForm显示层实现
    /// </summary>
    public class WinFormDisplayLayer : IDisplayLayer
    {
        private MainForm mainForm;

        /// <summary>
        /// 构造函数
        /// </summary>
        public WinFormDisplayLayer()
        {
            // 在UI线程中创建主表单
            if (Application.OpenForms.Count == 0)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
            }

            mainForm = new MainForm();
        }

        /// <summary>
        /// 显示欢迎界面
        /// </summary>
        public void ShowWelcome()
        {
            // WinForm不需要单独的欢迎界面，直接显示主表单
            Application.Run(mainForm);
        }

        /// <summary>
        /// 显示网卡列表并让用户选择
        /// </summary>
        /// <param name="nicList">网卡列表</param>
        /// <param name="defaultNicName">默认网卡名称</param>
        /// <returns>选择的网卡</returns>
        public NicInfo SelectNetworkAdapter(List<NicInfo> nicList, string defaultNicName)
        {
            // 这个方法在WinForm中不需要，因为表单已经显示，用户可以直接选择
            return null;
        }

        /// <summary>
        /// 显示网络配置列表并让用户选择
        /// </summary>
        /// <param name="configs">网络配置列表</param>
        /// <returns>选择的网络配置</returns>
        public NetworkConfig SelectNetworkConfig(List<NetworkConfig> configs)
        {
            // 这个方法在WinForm中不需要，因为表单已经显示，用户可以直接选择
            return null;
        }

        /// <summary>
        /// 显示当前网络配置
        /// </summary>
        /// <param name="nicName">网卡名称</param>
        /// <param name="config">网络配置</param>
        public void ShowCurrentConfig(string nicName, NetworkConfig config)
        {
            // 在WinForm中更新当前配置显示
            mainForm.UpdateCurrentConfig(config);
        }

        /// <summary>
        /// 显示配置应用进度和结果
        /// </summary>
        /// <param name="nicName">网卡名称</param>
        /// <param name="config">网络配置</param>
        /// <returns>用户是否确认应用配置</returns>
        public bool ShowConfigApplication(string nicName, NetworkConfig config)
        {
            // 在WinForm中更新目标配置显示
            mainForm.UpdateTargetConfig(config);
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
            // 在WinForm中更新配置比较
            mainForm.UpdateCurrentConfig(currentConfig);
            mainForm.UpdateTargetConfig(targetConfig);
            return true;
        }

        /// <summary>
        /// 显示配置应用结果
        /// </summary>
        /// <param name="success">是否成功</param>
        /// <param name="nicName">网卡名称</param>
        /// <param name="configName">配置名称</param>
        public void ShowApplicationResult(bool success, string nicName, string configName)
        {
            // 在WinForm中显示结果消息
            string message = success ?
                $"成功将网卡 {nicName} 的配置切换为 {configName}！" :
                $"将网卡 {nicName} 的配置切换为 {configName} 失败！";
            mainForm.ShowMessage(message, success ? MessageBoxIcon.Information : MessageBoxIcon.Error);
        }

        /// <summary>
        /// 显示错误信息
        /// </summary>
        /// <param name="message">错误信息</param>
        public void ShowError(string message)
        {
            mainForm.ShowMessage(message, MessageBoxIcon.Error);
        }

        /// <summary>
        /// 显示信息
        /// </summary>
        /// <param name="message">信息内容</param>
        public void ShowInfo(string message)
        {
            mainForm.ShowMessage(message, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 等待用户按键
        /// </summary>
        public void WaitForKeyPress()
        {
            // WinForm中不需要，因为表单是模态运行的
        }
    }
}