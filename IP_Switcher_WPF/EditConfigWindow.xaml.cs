using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using IP_Switcher.Models;

namespace IP_Switcher_WPF
{
    /// <summary>
    /// EditConfigWindow.xaml 的交互逻辑
    /// </summary>
    public partial class EditConfigWindow : Window
    {
        public NetworkConfig Config { get; private set; }
        private readonly List<NetworkConfig> _existingConfigs;

        public EditConfigWindow(NetworkConfig config, List<NetworkConfig> existingConfigs)
        {
            InitializeComponent();
            Config = config;
            _existingConfigs = existingConfigs;
            SetupEventHandlers();
            LoadConfigData();
        }

        /// <summary>
        /// 设置事件处理程序
        /// </summary>
        private void SetupEventHandlers()
        {
            okButton.Click += OkButton_Click;
            cancelButton.Click += CancelButton_Click;
        }

        /// <summary>
        /// 加载配置数据到表单
        /// </summary>
        private void LoadConfigData()
        {
            try
            {
                // 设置表单标题，区分新增和编辑
                Title = string.IsNullOrEmpty(Config.IPAddress) && string.IsNullOrEmpty(Config.SubnetMask) ? "新增网络配置" : "编辑网络配置";

                // 配置名称
                nameTextBox.Text = Config.Name;

                // IP地址
                ipTextBox.Text = Config.IPAddress;

                // 子网掩码
                maskTextBox.Text = Config.SubnetMask;

                // 默认网关
                gatewayTextBox.Text = Config.DefaultGateway;

                // DNS服务器 - 多行文本框
                if (Config.DnsServers != null)
                {
                    // 将DNS服务器列表转换为每行一个
                    dnsTextBox.Text = string.Join(Environment.NewLine, Config.DnsServers);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载配置数据失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 保存配置数据
        /// </summary>
        private void SaveConfigData()
        {
            try
            {
                // 配置名称
                Config.Name = nameTextBox.Text.Trim();

                // IP地址
                Config.IPAddress = ipTextBox.Text.Trim();

                // 子网掩码
                Config.SubnetMask = maskTextBox.Text.Trim();

                // 默认网关
                Config.DefaultGateway = gatewayTextBox.Text.Trim();

                // DNS服务器 - 多行文本框
                Config.DnsServers = new List<string>();

                // 按行分割，去除空行和空格
                string[] dnsLines = dnsTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in dnsLines)
                {
                    string dns = line.Trim();
                    if (!string.IsNullOrEmpty(dns))
                    {
                        Config.DnsServers.Add(dns);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存配置数据失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        /// <summary>
        /// 验证IP地址格式
        /// </summary>
        /// <param name="ipAddress">要验证的IP地址</param>
        /// <returns>是否为有效的IP地址</returns>
        private bool IsValidIpAddress(string ipAddress)
        {
            return IPAddress.TryParse(ipAddress, out _);
        }

        /// <summary>
        /// 验证配置名称是否唯一
        /// </summary>
        /// <param name="name">配置名称</param>
        /// <returns>是否唯一</returns>
        private bool IsConfigNameUnique(string name)
        {
            if (_existingConfigs == null) return true;

            // 排除当前编辑的配置
            return !_existingConfigs.Any(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && !ReferenceEquals(c, Config));
        }

        /// <summary>
        /// 验证配置数据
        /// </summary>
        /// <returns>是否验证通过</returns>
        private bool ValidateConfigData()
        {
            try
            {
                // 配置名称不能为空
                string configName = nameTextBox.Text.Trim();
                if (string.IsNullOrEmpty(configName))
                {
                    MessageBox.Show("配置名称不能为空", "验证错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    nameTextBox.Focus();
                    return false;
                }

                // 验证配置名称唯一性
                if (!IsConfigNameUnique(configName))
                {
                    MessageBox.Show("配置名称已存在，请使用其他名称", "验证错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    nameTextBox.Focus();
                    return false;
                }

                // 如果不是DHCP配置，需要验证IP相关参数
                if (!string.IsNullOrEmpty(ipTextBox.Text.Trim()))
                {
                    // 验证IP地址格式
                    string ipAddress = ipTextBox.Text.Trim();
                    if (!IsValidIpAddress(ipAddress))
                    {
                        MessageBox.Show("IP地址格式不正确", "验证错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        ipTextBox.Focus();
                        return false;
                    }

                    // 验证子网掩码格式
                    string subnetMask = maskTextBox.Text.Trim();
                    if (string.IsNullOrEmpty(subnetMask))
                    {
                        MessageBox.Show("子网掩码不能为空", "验证错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        maskTextBox.Focus();
                        return false;
                    }

                    if (!IsValidIpAddress(subnetMask))
                    {
                        MessageBox.Show("子网掩码格式不正确", "验证错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        maskTextBox.Focus();
                        return false;
                    }

                    // 验证默认网关格式（如果填写）
                    string gateway = gatewayTextBox.Text.Trim();
                    if (!string.IsNullOrEmpty(gateway) && !IsValidIpAddress(gateway))
                    {
                        MessageBox.Show("默认网关格式不正确", "验证错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        gatewayTextBox.Focus();
                        return false;
                    }
                }

                // 验证DNS服务器格式
                string[] dnsLines = dnsTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in dnsLines)
                {
                    string dns = line.Trim();
                    if (!string.IsNullOrEmpty(dns) && !IsValidIpAddress(dns))
                    {
                        MessageBox.Show($"DNS服务器 '{dns}' 格式不正确", "验证错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        dnsTextBox.Focus();
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"验证配置数据失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// 确定按钮点击事件
        /// </summary>
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ValidateConfigData())
                {
                    SaveConfigData();
                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"操作失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 取消按钮点击事件
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}