using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using IP_Switcher;
using IP_Switcher.Models;

namespace IP_Switcher_WPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        // 常量定义
        private const string DHCP_CONFIG_NAME = "DHCP自动获取";
        
        private NetworkManager _networkManager;
        private ConfigManager _configManager;
        private Logger _logger;
        private ObservableCollection<NetworkConfig> _configs;

        public MainWindow()
        {
            InitializeComponent();
            InitializeApp();
            SetupEventHandlers();
        }

        /// <summary>
        /// 初始化应用程序
        /// </summary>
        private void InitializeApp()
        {
            try
            {
                // 创建所有依赖对象
                _networkManager = new NetworkManager();
                _configManager = new ConfigManager();
                _logger = new Logger();
                _logger.Info("应用程序初始化开始");

                // 加载初始数据
                LoadNicList();
                LoadConfigList();
                
                _logger.Info("应用程序初始化完成");
            }
            catch (Exception ex)
            {
                _logger.Error("初始化应用失败: " + ex.Message);
                MessageBox.Show($"初始化应用失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 设置事件处理程序
        /// </summary>
        private void SetupEventHandlers()
        {
            addButton.Click += AddButton_Click;
            editButton.Click += EditButton_Click;
            deleteButton.Click += DeleteButton_Click;
            applyButton.Click += ApplyButton_Click;
            exitButton.Click += ExitButton_Click;
            nicComboBox.SelectionChanged += NicComboBox_SelectionChanged;
        }

        /// <summary>
        /// 加载网卡列表
        /// </summary>
        private void LoadNicList()
        {
            try
            {
                _logger.Info("开始加载网卡列表");
                var nicList = _networkManager.GetAllNetworkAdapters();
                nicComboBox.Items.Clear();

                // 添加网卡名称到下拉框
                foreach (var nic in nicList)
                {
                    nicComboBox.Items.Add(nic.Name);
                }

                _logger.Info($"成功加载 {nicComboBox.Items.Count} 个网卡");
                
                if (nicComboBox.Items.Count > 0)
                {
                    // 从配置文件读取上次选中的网卡
                    string lastNic = _configManager.GetLastNic();
                    if (!string.IsNullOrEmpty(lastNic))
                    {
                        // 查找上次选中的网卡并选中
                        int index = nicComboBox.Items.IndexOf(lastNic);
                        if (index != -1)
                        {
                            nicComboBox.SelectedIndex = index;
                            _logger.Info($"恢复上次选中的网卡: {lastNic}");
                        }
                        else
                        {
                            // 如果上次选中的网卡不存在，默认选中第一个
                            nicComboBox.SelectedIndex = 0;
                            // 保存当前选中的网卡为默认网卡
                            _configManager.SetLastNic(nicComboBox.SelectedItem as string);
                            _logger.Info($"上次选中的网卡 {lastNic} 不存在，默认选中第一个网卡");
                        }
                    }
                    else
                    {
                        // 没有上次选中的网卡，默认选中第一个
                        nicComboBox.SelectedIndex = 0;
                        // 保存当前选中的网卡为默认网卡
                        _configManager.SetLastNic(nicComboBox.SelectedItem as string);
                        _logger.Info("首次启动，默认选中第一个网卡");
                    }
                    
                    // 触发选中事件以加载默认网卡的配置
                    OnNicSelected();
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"加载网卡列表失败: {ex.Message}");
                MessageBox.Show($"加载网卡列表失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 处理网卡选中事件
        /// </summary>
        private void OnNicSelected()
        {
            string selectedNicName = nicComboBox.SelectedItem as string;
            if (!string.IsNullOrEmpty(selectedNicName))
            {
                try
                {
                    _logger.Info($"网卡选中变化: {selectedNicName}");
                    // 获取选中网卡的当前配置
                    var currentConfig = _networkManager.GetCurrentIpConfig(selectedNicName);
                    
                    // 保存选中的网卡为默认网卡
                    _configManager.SetLastNic(selectedNicName);
                    
                    // 更新当前配置显示
                    UpdateCurrentConfigDisplay(currentConfig);
                    _logger.Info($"成功获取网卡 {selectedNicName} 的当前配置");
                }
                catch (Exception ex)
                {
                    _logger.Error($"获取网卡 {selectedNicName} 当前配置失败: " + ex.Message);
                    // 处理异常但不显示用户消息，避免频繁弹出
                    currentConfigText.Text = "获取当前配置失败";
                }
            }
        }
        
        /// <summary>
        /// 更新当前配置显示
        /// </summary>
        /// <param name="config">当前网络配置</param>
        private void UpdateCurrentConfigDisplay(NetworkConfig config)
        {
            if (config == null)
            {
                currentConfigText.Text = "无有效配置";
                return;
            }
            
            // 构建配置信息字符串
            string configInfo = string.Empty;
            
            if (!string.IsNullOrEmpty(config.IPAddress))
            {
                configInfo += $"IP地址: {config.IPAddress}\n";
                configInfo += $"子网掩码: {config.SubnetMask}\n";
                configInfo += $"默认网关: {config.DefaultGateway}\n";
                
                if (config.DnsServers != null && config.DnsServers.Count > 0)
                {
                    configInfo += $"DNS服务器: {string.Join("；", config.DnsServers)}\n";
                }
                else
                {
                    configInfo += "DNS服务器: 无\n";
                }
            }
            else
            {
                configInfo += "当前使用 DHCP 自动获取 IP 配置\n";
            }
            
            // 移除最后的换行符
            currentConfigText.Text = configInfo.TrimEnd('\n');
        }

        /// <summary>
        /// 加载配置列表
        /// </summary>
        private void LoadConfigList()
        {
            try
            {
                _logger.Info("开始加载配置列表");
                // 从配置文件加载配置
                var configList = _configManager.LoadConfig();
                
                // 创建一个默认的DHCP配置项
                NetworkConfig dhcpConfig = new NetworkConfig
                {
                    Name = DHCP_CONFIG_NAME
                };
                
                // 检查是否已存在DHCP配置项，如果不存在则添加
                if (!configList.Any(c => c.Name == DHCP_CONFIG_NAME))
                {
                    configList.Insert(0, dhcpConfig);
                }
                
                // 转换为ObservableCollection以支持自动UI更新
                _configs = new ObservableCollection<NetworkConfig>(configList);
                configListView.ItemsSource = _configs;
                _logger.Info($"成功加载 {_configs.Count} 个配置项");
            }
            catch (Exception ex)
            {
                _logger.Error($"加载配置列表失败: {ex.Message}");
                MessageBox.Show($"加载配置列表失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 添加配置按钮点击事件
        /// </summary>
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _logger.Info("开始添加新配置");
                EditConfigWindow editConfigWindow = new EditConfigWindow(new NetworkConfig(), _configs.ToList());
                if (editConfigWindow.ShowDialog() == true)
                {
                    NetworkConfig newConfig = editConfigWindow.Config;
                    _configs.Add(newConfig);
                    // 保存配置时排除DHCP配置项
                    _configManager.SaveConfig(_configs.Where(c => c.Name != DHCP_CONFIG_NAME).ToList());
                    _logger.Info($"成功添加配置: {newConfig.Name}");
                }
                else
                {
                    _logger.Info("用户取消添加配置");
                }
            }
            catch (Exception ex)
            {
                _logger.Error("添加配置失败: " + ex.Message);
                MessageBox.Show($"添加配置失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 编辑配置按钮点击事件
        /// </summary>
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (configListView.SelectedItem is NetworkConfig selectedConfig)
                {
                    _logger.Info($"开始编辑配置: {selectedConfig.Name}");
                    // 检查是否是DHCP配置项，如果是则不允许编辑
                    if (IsDhcpConfig(selectedConfig))
                    {
                        _logger.Info("尝试编辑DHCP配置，已拒绝");
                        MessageBox.Show("DHCP配置项不能被编辑", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    
                    int index = _configs.IndexOf(selectedConfig);
                    if (index >= 0)
                    {
                        EditConfigWindow editConfigWindow = new EditConfigWindow(selectedConfig, _configs.ToList());
                        if (editConfigWindow.ShowDialog() == true)
                        {
                            NetworkConfig updatedConfig = editConfigWindow.Config;
                            _configs[index] = updatedConfig;
                            // 保存配置时排除DHCP配置项
                            _configManager.SaveConfig(_configs.Where(c => c.Name != DHCP_CONFIG_NAME).ToList());
                            _logger.Info($"成功编辑配置: {updatedConfig.Name}");
                        }
                        else
                        {
                            _logger.Info("用户取消编辑配置");
                        }
                    }
                }
                else
                {
                    _logger.Info("尝试编辑配置，但未选择任何配置项");
                    MessageBox.Show("请选择要编辑的配置", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("编辑配置失败: " + ex.Message);
                MessageBox.Show($"编辑配置失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 删除配置按钮点击事件
        /// </summary>
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (configListView.SelectedItem is NetworkConfig selectedConfig)
                {
                    _logger.Info($"开始删除配置: {selectedConfig.Name}");
                    // 检查是否是DHCP配置项，如果是则不允许删除
                    if (IsDhcpConfig(selectedConfig))
                    {
                        _logger.Info("尝试删除DHCP配置，已拒绝");
                        MessageBox.Show("DHCP配置项不能被删除", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    
                    if (MessageBox.Show($"确定要删除配置 '{selectedConfig.Name}' 吗?", "确认删除", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        _configs.Remove(selectedConfig);
                        // 保存配置时排除DHCP配置项
                        _configManager.SaveConfig(_configs.Where(c => c.Name != DHCP_CONFIG_NAME).ToList());
                        _logger.Info($"成功删除配置: {selectedConfig.Name}");
                    }
                    else
                    {
                        _logger.Info("用户取消删除配置");
                    }
                }
                else
                {
                    _logger.Info("尝试删除配置，但未选择任何配置项");
                    MessageBox.Show("请选择要删除的配置", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("删除配置失败: " + ex.Message);
                MessageBox.Show($"删除配置失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 应用配置按钮点击事件😊
        /// </summary>
        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string selectedNicName = nicComboBox.SelectedItem as string;

                if (string.IsNullOrEmpty(selectedNicName) || !(configListView.SelectedItem is NetworkConfig selectedConfig))
                {
                    _logger.Info("尝试应用配置，但未选择网卡或配置");
                    MessageBox.Show("请选择网卡和配置", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                _logger.Info($"开始应用配置 '{selectedConfig.Name}' 到网卡 '{selectedNicName}'");
                if (MessageBox.Show($"确定要将配置 '{selectedConfig.Name}' 应用到网卡 '{selectedNicName}' 吗?", "确认应用", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    bool result;
                    
                    if (IsDhcpConfig(selectedConfig))
                    {
                        // 如果是DHCP配置项，创建一个空配置来触发DHCP设置
                        NetworkConfig dhcpConfig = new NetworkConfig
                        {
                            NicName = selectedNicName
                        };
                        _logger.Info($"正在为网卡 '{selectedNicName}' 设置DHCP");
                        result = _networkManager.SetIpConfig(selectedNicName, dhcpConfig);
                    }
                    else
                    {
                        // 如果是静态配置，正常设置
                        selectedConfig.NicName = selectedNicName;
                        _logger.Info($"正在为网卡 '{selectedNicName}' 设置静态IP: {selectedConfig.IPAddress}");
                        result = _networkManager.SetIpConfig(selectedNicName, selectedConfig);
                    }
                    
                    if (result)
                    {
                        _logger.Info($"配置 '{selectedConfig.Name}' 应用到网卡 '{selectedNicName}' 成功");
                        MessageBox.Show("配置应用成功", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                        // 应用成功后更新当前配置显示
                        OnNicSelected();
                    }
                    else
                    {
                        _logger.Error($"配置 '{selectedConfig.Name}' 应用到网卡 '{selectedNicName}' 失败");
                        MessageBox.Show("配置应用失败，可能需要管理员权限", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    _logger.Info("用户取消应用配置");
                }
            }
            catch (Exception ex)
            {
                _logger.Error("应用配置失败: " + ex.Message);
                MessageBox.Show($"应用配置失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 退出按钮点击事件
        /// </summary>
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        
        /// <summary>
        /// 检查配置是否为DHCP配置
        /// </summary>
        /// <param name="config">要检查的网络配置</param>
        /// <returns>如果是DHCP配置则返回true，否则返回false</returns>
        private bool IsDhcpConfig(NetworkConfig config)
        {
            return config != null && config.Name == DHCP_CONFIG_NAME;
        }

        /// <summary>
        /// 网卡选择变化事件
        /// </summary>
        private void NicComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 当网卡选择变化时调用OnNicSelected更新配置
            OnNicSelected();
        }
    }
}