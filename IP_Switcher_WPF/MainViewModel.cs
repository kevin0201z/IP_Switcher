using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using IP_Switcher;
using IP_Switcher.Interfaces;
using IP_Switcher.Models;

namespace IP_Switcher_WPF
{
    /// <summary>
    /// 主窗口视图模型
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        // 常量定义
        private const string DHCP_CONFIG_NAME = "DHCP自动获取";

        private readonly INetworkManager _networkManager;
        private readonly IConfigManager _configManager;
        private readonly ILogger _logger;

        /// <summary>
        /// 属性变化事件
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 触发属性变化事件
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region 属性

        /// <summary>
        /// 网卡列表
        /// </summary>
        public ObservableCollection<string> NicList { get; private set; }

        /// <summary>
        /// 选中的网卡
        /// </summary>
        private string _selectedNic;
        public string SelectedNic
        {
            get { return _selectedNic; }
            set
            {
                if (_selectedNic != value)
                {
                    _selectedNic = value;
                    OnPropertyChanged(nameof(SelectedNic));
                    OnNicSelected();
                }
            }
        }

        /// <summary>
        /// 当前配置显示文本
        /// </summary>
        private string _currentConfigText;
        public string CurrentConfigText
        {
            get { return _currentConfigText; }
            set
            {
                if (_currentConfigText != value)
                {
                    _currentConfigText = value;
                    OnPropertyChanged(nameof(CurrentConfigText));
                }
            }
        }

        /// <summary>
        /// 网络配置列表
        /// </summary>
        public ObservableCollection<NetworkConfig> Configs { get; private set; }

        /// <summary>
        /// 选中的网络配置
        /// </summary>
        private NetworkConfig _selectedConfig;
        public NetworkConfig SelectedConfig
        {
            get { return _selectedConfig; }
            set
            {
                if (_selectedConfig != value)
                {
                    _selectedConfig = value;
                    OnPropertyChanged(nameof(SelectedConfig));
                }
            }
        }

        #endregion

        #region 命令

        /// <summary>
        /// 添加配置命令
        /// </summary>
        public ICommand AddConfigCommand { get; private set; }

        /// <summary>
        /// 编辑配置命令
        /// </summary>
        public ICommand EditConfigCommand { get; private set; }

        /// <summary>
        /// 删除配置命令
        /// </summary>
        public ICommand DeleteConfigCommand { get; private set; }

        /// <summary>
        /// 应用配置命令
        /// </summary>
        public ICommand ApplyConfigCommand { get; private set; }

        /// <summary>
        /// 退出命令
        /// </summary>
        public ICommand ExitCommand { get; private set; }

        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="networkManager">网络管理器</param>
        /// <param name="configManager">配置管理器</param>
        /// <param name="logger">日志记录器</param>
        public MainViewModel(INetworkManager networkManager, IConfigManager configManager, ILogger logger)
        {
            _networkManager = networkManager;
            _configManager = configManager;
            _logger = logger;

            // 初始化集合
            NicList = new ObservableCollection<string>();
            Configs = new ObservableCollection<NetworkConfig>();

            // 初始化命令
            InitializeCommands();

            // 初始化应用程序
            InitializeApp();
        }

        /// <summary>
        /// 初始化命令
        /// </summary>
        private void InitializeCommands()
        {
            AddConfigCommand = new RelayCommand(AddConfig);
            EditConfigCommand = new RelayCommand(EditConfig, CanEditConfig);
            DeleteConfigCommand = new RelayCommand(DeleteConfig, CanDeleteConfig);
            ApplyConfigCommand = new RelayCommand(ApplyConfig, CanApplyConfig);
            ExitCommand = new RelayCommand(Exit);
        }

        /// <summary>
        /// 初始化应用程序
        /// </summary>
        private void InitializeApp()
        {
            try
            {
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
        /// 加载网卡列表
        /// </summary>
        private void LoadNicList()
        {
            try
            {
                _logger.Info("开始加载网卡列表");
                var nicList = _networkManager.GetAllNetworkAdapters();
                NicList.Clear();

                // 添加网卡名称到下拉框
                foreach (var nic in nicList)
                {
                    NicList.Add(nic.Name);
                }

                _logger.Info($"成功加载 {NicList.Count} 个网卡");

                if (NicList.Count > 0)
                {
                    // 从配置文件读取上次选中的网卡
                    string lastNic = _configManager.GetLastNic();
                    if (!string.IsNullOrEmpty(lastNic))
                    {
                        // 查找上次选中的网卡并选中
                        int index = NicList.IndexOf(lastNic);
                        if (index != -1)
                        {
                            SelectedNic = lastNic;
                            _logger.Info($"恢复上次选中的网卡: {lastNic}");
                        }
                        else
                        {
                            // 如果上次选中的网卡不存在，默认选中第一个
                            SelectedNic = NicList[0];
                            // 保存当前选中的网卡为默认网卡
                            _configManager.SetLastNic(SelectedNic);
                            _logger.Info($"上次选中的网卡 {lastNic} 不存在，默认选中第一个网卡");
                        }
                    }
                    else
                    {
                        // 没有上次选中的网卡，默认选中第一个
                        SelectedNic = NicList[0];
                        // 保存当前选中的网卡为默认网卡
                        _configManager.SetLastNic(SelectedNic);
                        _logger.Info("首次启动，默认选中第一个网卡");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"加载网卡列表失败: {ex.Message}");
                MessageBox.Show($"加载网卡列表失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 加载配置列表
        /// </summary>
        private void LoadConfigList()
        {
            try
            {
                _logger.Info("开始加载配置列表");
                Configs.Clear();

                // 添加DHCP配置项
                Configs.Add(new NetworkConfig
                {
                    Name = DHCP_CONFIG_NAME,
                    IPAddress = "",
                    SubnetMask = "",
                    DefaultGateway = "",
                    DnsServers = new System.Collections.Generic.List<string>()
                });

                // 获取保存的配置列表
                var savedConfigs = _configManager.LoadConfig();

                // 添加保存的配置
                foreach (var config in savedConfigs)
                {
                    Configs.Add(config);
                }
                
                _logger.Info($"成功加载 {Configs.Count} 个配置");
            }
            catch (Exception ex)
            {
                _logger.Error($"加载配置列表失败: {ex.Message}");
                MessageBox.Show($"加载配置列表失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 处理网卡选中事件
        /// </summary>
        private void OnNicSelected()
        {
            string selectedNicName = SelectedNic;
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
                    _logger.Error($"获取网卡配置失败: {ex.Message}");
                    MessageBox.Show($"获取网卡配置失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// 更新当前配置显示
        /// </summary>
        /// <param name="config">网络配置</param>
        public void UpdateCurrentConfigDisplay(NetworkConfig config)
        {
            if (config == null)
            {
                CurrentConfigText = "无法获取当前配置";
                return;
            }

            // 如果是DHCP配置
            if (string.IsNullOrEmpty(config.IPAddress))
            {
                CurrentConfigText = "当前使用: DHCP自动获取";
            }
            else
            {
                // 静态IP配置
                string configText = $"当前使用: 静态IP\nIP地址: {config.IPAddress}\n子网掩码: {config.SubnetMask}";
                if (!string.IsNullOrEmpty(config.DefaultGateway))
                {
                    configText += $"\n默认网关: {config.DefaultGateway}";
                }
                if (config.DnsServers != null && config.DnsServers.Count > 0)
                {
                    configText += $"\nDNS服务器: {string.Join(", ", config.DnsServers)}";
                }
                CurrentConfigText = configText;
            }
        }

        #region 命令处理方法

        /// <summary>
        /// 添加配置
        /// </summary>
        private void AddConfig()
        {
            try
            {
                _logger.Info("开始添加新配置");
                EditConfigWindow editConfigWindow = new EditConfigWindow(new NetworkConfig(), Configs.ToList());
                if (editConfigWindow.ShowDialog() == true)
                {
                    NetworkConfig newConfig = editConfigWindow.Config;
                    Configs.Add(newConfig);
                    // 保存配置时排除DHCP配置项
                    _configManager.SaveConfig(Configs.Where(c => c.Name != DHCP_CONFIG_NAME).ToList());
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
        /// 编辑配置
        /// </summary>
        private void EditConfig()
        {
            try
            {
                if (SelectedConfig != null)
                {
                    _logger.Info($"开始编辑配置: {SelectedConfig.Name}");
                    // 检查是否是DHCP配置项，如果是则不允许编辑
                    if (IsDhcpConfig(SelectedConfig))
                    {
                        _logger.Info("尝试编辑DHCP配置，已拒绝");
                        MessageBox.Show("DHCP配置项不能被编辑", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    int index = Configs.IndexOf(SelectedConfig);
                    if (index >= 0)
                    {
                        EditConfigWindow editConfigWindow = new EditConfigWindow(SelectedConfig, Configs.ToList());
                        if (editConfigWindow.ShowDialog() == true)
                        {
                            NetworkConfig updatedConfig = editConfigWindow.Config;
                            // 替换ObservableCollection中的项，确保UI刷新
                            Configs[index] = null;
                            Configs[index] = updatedConfig;
                            // 保存配置时排除DHCP配置项
                            _configManager.SaveConfig(Configs.Where(c => c.Name != DHCP_CONFIG_NAME).ToList());
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
        /// 删除配置
        /// </summary>
        private void DeleteConfig()
        {
            try
            {
                if (SelectedConfig != null)
                {
                    _logger.Info($"开始删除配置: {SelectedConfig.Name}");
                    // 检查是否是DHCP配置项，如果是则不允许删除
                    if (IsDhcpConfig(SelectedConfig))
                    {
                        _logger.Info("尝试删除DHCP配置，已拒绝");
                        MessageBox.Show("DHCP配置项不能被删除", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    if (MessageBox.Show($"确定要删除配置 '{SelectedConfig.Name}' 吗?", "确认删除", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        // 保存要删除的配置名称
                        string configName = SelectedConfig.Name;
                        Configs.Remove(SelectedConfig);
                        // 保存配置时排除DHCP配置项
                        _configManager.SaveConfig(Configs.Where(c => c.Name != DHCP_CONFIG_NAME).ToList());
                        _logger.Info($"成功删除配置: {configName}");
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
        /// 应用配置
        /// </summary>
        private void ApplyConfig()
        {
            try
            {
                string selectedNicName = SelectedNic;

                if (string.IsNullOrEmpty(selectedNicName) || SelectedConfig == null)
                {
                    _logger.Info("尝试应用配置，但未选择网卡或配置");
                    MessageBox.Show("请选择网卡和配置", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                _logger.Info($"开始应用配置 '{SelectedConfig.Name}' 到网卡 '{selectedNicName}'");
                if (MessageBox.Show($"确定要将配置 '{SelectedConfig.Name}' 应用到网卡 '{selectedNicName}' 吗?", "确认应用", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    // 保存原始配置（用于还原）
                    NetworkConfig originalConfig = _networkManager.GetCurrentIpConfig(selectedNicName);

                    bool result;
                    NetworkConfig appliedConfig;

                    if (IsDhcpConfig(SelectedConfig))
                    {
                        // 如果是DHCP配置项，创建一个空配置来触发DHCP设置
                        NetworkConfig dhcpConfig = new NetworkConfig
                        {
                            NicName = selectedNicName
                        };
                        appliedConfig = dhcpConfig;
                        _logger.Info($"正在为网卡 '{selectedNicName}' 设置DHCP");
                        result = _networkManager.SetIpConfig(selectedNicName, dhcpConfig);
                    }
                    else
                    {
                        // 如果是静态配置，正常设置
                        SelectedConfig.NicName = selectedNicName;
                        appliedConfig = SelectedConfig;
                        _logger.Info($"正在为网卡 '{selectedNicName}' 设置静态IP: {SelectedConfig.IPAddress}");
                        result = _networkManager.SetIpConfig(selectedNicName, SelectedConfig);
                    }

                    if (result)
                    {
                        _logger.Info($"配置 '{SelectedConfig.Name}' 应用到网卡 '{selectedNicName}' 成功");

                        // 打开确认对话框，显示倒计时
                        ConfirmDialog confirmDialog = new ConfirmDialog(_networkManager, selectedNicName, originalConfig, appliedConfig);
                        bool? dialogResult = confirmDialog.ShowDialog();

                        if (dialogResult == true)
                        {
                            // 用户确认，保持新配置
                            _logger.Info($"用户确认了配置 '{SelectedConfig.Name}'");
                            MessageBox.Show("配置应用成功并已确认", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            // 用户取消或倒计时结束，已自动还原
                            _logger.Info($"配置 '{SelectedConfig.Name}' 未被确认，已自动还原");
                            MessageBox.Show("配置已还原", "已还原", MessageBoxButton.OK, MessageBoxImage.Information);
                        }

                        // 应用成功后更新当前配置显示
                        // 重新加载网卡当前配置
                        var currentConfig = _networkManager.GetCurrentIpConfig(selectedNicName);
                        UpdateCurrentConfigDisplay(currentConfig);
                    }
                    else
                    {
                        _logger.Error($"配置 '{SelectedConfig.Name}' 应用到网卡 '{selectedNicName}' 失败");
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
        /// 退出应用
        /// </summary>
        private void Exit()
        {
            // 获取当前窗口
            var window = Application.Current.MainWindow;
            window?.Close();
        }

        #endregion

        #region 命令可用性检查

        /// <summary>
        /// 检查是否可以编辑配置
        /// </summary>
        /// <returns>是否可以编辑</returns>
        private bool CanEditConfig()
        {
            return SelectedConfig != null && !IsDhcpConfig(SelectedConfig);
        }

        /// <summary>
        /// 检查是否可以删除配置
        /// </summary>
        /// <returns>是否可以删除</returns>
        private bool CanDeleteConfig()
        {
            return SelectedConfig != null && !IsDhcpConfig(SelectedConfig);
        }

        /// <summary>
        /// 检查是否可以应用配置
        /// </summary>
        /// <returns>是否可以应用</returns>
        private bool CanApplyConfig()
        {
            return !string.IsNullOrEmpty(SelectedNic) && SelectedConfig != null;
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 检查配置是否为DHCP配置
        /// </summary>
        /// <param name="config">要检查的网络配置</param>
        /// <returns>如果是DHCP配置则返回true，否则返回false</returns>
        private bool IsDhcpConfig(NetworkConfig config)
        {
            return config != null && config.Name == DHCP_CONFIG_NAME;
        }

        #endregion
    }
}