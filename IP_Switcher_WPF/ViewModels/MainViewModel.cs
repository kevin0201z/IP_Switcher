using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IP_Switcher;
using IP_Switcher.Interfaces;
using IP_Switcher.Models;
using IP_Switcher_WPF.Services;

namespace IP_Switcher_WPF.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private const string DHCP_CONFIG_NAME = "DHCP自动获取";

        private readonly INetworkManager _networkManager;
        private readonly IConfigManager _configManager;
        private readonly ILogger _logger;
        private readonly IDialogService _dialogService;
        private readonly INotificationService _notificationService;

        [ObservableProperty]
        private ObservableCollection<NicDisplayItem> _nicList = new ObservableCollection<NicDisplayItem>();

        private NicDisplayItem _selectedNicItem;
        public NicDisplayItem SelectedNicItem
        {
            get => _selectedNicItem;
            set
            {
                if (SetProperty(ref _selectedNicItem, value) && value != null)
                {
                    _ = OnNicSelectedAsync();
                }
            }
        }

        public string SelectedNic => SelectedNicItem?.Name;

        [ObservableProperty]
        private string _currentConfigText;

        [ObservableProperty]
        private ObservableCollection<NetworkConfig> _configs = new ObservableCollection<NetworkConfig>();

        [ObservableProperty]
        private NetworkConfig _selectedConfig;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private bool _isInitialized;

        [ObservableProperty]
        private string _statusMessage = "加载中";

        public IAsyncRelayCommand AddConfigCommand { get; }
        public IAsyncRelayCommand EditConfigCommand { get; }
        public IAsyncRelayCommand DeleteConfigCommand { get; }
        public IAsyncRelayCommand ApplyConfigCommand { get; }
        public IRelayCommand ExitCommand { get; }

        public MainViewModel(
            INetworkManager networkManager,
            IConfigManager configManager,
            ILogger logger,
            IDialogService dialogService,
            INotificationService notificationService)
        {
            _networkManager = networkManager;
            _configManager = configManager;
            _logger = logger;
            _dialogService = dialogService;
            _notificationService = notificationService;

            AddConfigCommand = new AsyncRelayCommand(AddConfigAsync);
            EditConfigCommand = new AsyncRelayCommand(EditConfigAsync, CanEditConfig);
            DeleteConfigCommand = new AsyncRelayCommand(DeleteConfigAsync, CanDeleteConfig);
            ApplyConfigCommand = new AsyncRelayCommand(ApplyConfigAsync, CanApplyConfig);
            ExitCommand = new CommunityToolkit.Mvvm.Input.RelayCommand(Exit);

            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            IsBusy = true;
            StatusMessage = "正在初始化";
            try
            {
                _logger.Info("应用程序初始化开始");

                await Task.Run(async () =>
                {
                    var configs = _configManager.LoadConfig();
                    var nicList = _networkManager.GetAllNetworkAdapters();
                    var lastNic = _configManager.GetLastNic();

                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        LoadConfigList(configs);
                        LoadNicList(nicList, lastNic);
                    });
                });

                IsInitialized = true;
                _logger.Info("应用程序初始化完成");
            }
            catch (Exception ex)
            {
                _logger.Error("初始化应用失败: " + ex.Message);
                await _dialogService.ShowErrorAsync($"初始化应用失败: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void LoadConfigList(List<NetworkConfig> savedConfigs)
        {
            Configs.Clear();

            Configs.Add(new NetworkConfig
            {
                Name = DHCP_CONFIG_NAME,
                IPAddress = "",
                SubnetMask = "",
                DefaultGateway = "",
                DnsServers = new List<string>()
            });

            if (savedConfigs != null)
            {
                foreach (var config in savedConfigs)
                {
                    Configs.Add(config);
                }
            }

            _logger.Info($"成功加载 {Configs.Count} 个配置");
        }

        private void LoadNicList(List<NicInfo> nicList, string lastNic)
        {
            NicList.Clear();

            if (nicList != null)
            {
                foreach (var nic in nicList)
                {
                    NicList.Add(new NicDisplayItem(nic));
                }
            }

            _logger.Info($"成功加载 {NicList.Count} 个网卡");

            if (NicList.Count > 0)
            {
                if (!string.IsNullOrEmpty(lastNic))
                {
                    var item = NicList.FirstOrDefault(n => n.Name == lastNic);
                    if (item != null)
                    {
                        SelectedNicItem = item;
                        _logger.Info($"恢复上次选中的网卡: {lastNic}");
                    }
                    else
                    {
                        SelectedNicItem = NicList[0];
                        _configManager.SetLastNic(SelectedNic);
                        _logger.Info($"上次选中的网卡 {lastNic} 不存在，默认选中第一个网卡");
                    }
                }
                else
                {
                    SelectedNicItem = NicList[0];
                    _configManager.SetLastNic(SelectedNic);
                    _logger.Info("首次启动，默认选中第一个网卡");
                }
            }
        }

        private async Task OnNicSelectedAsync()
        {
            if (string.IsNullOrEmpty(SelectedNic)) return;

            try
            {
                _logger.Info($"网卡选中变化: {SelectedNic}");
                var currentConfig = await Task.Run(() => _networkManager.GetCurrentIpConfig(SelectedNic));
                _configManager.SetLastNic(SelectedNic);
                UpdateCurrentConfigDisplay(currentConfig);
                _logger.Info($"成功获取网卡 {SelectedNic} 的当前配置");
            }
            catch (Exception ex)
            {
                _logger.Error($"获取网卡配置失败: {ex.Message}");
                await _dialogService.ShowErrorAsync($"获取网卡配置失败: {ex.Message}");
            }
        }

        private void UpdateCurrentConfigDisplay(NetworkConfig config)
        {
            if (config == null)
            {
                CurrentConfigText = "无法获取当前配置";
                return;
            }

            if (string.IsNullOrEmpty(config.IPAddress))
            {
                CurrentConfigText = "当前使用: DHCP自动获取";
            }
            else
            {
                var configText = $"当前使用: 静态IP\nIP地址: {config.IPAddress}\n子网掩码: {config.SubnetMask}";
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

        private async Task AddConfigAsync()
        {
            try
            {
                _logger.Info("开始添加新配置");
                var editConfigWindow = new EditConfigWindow(new NetworkConfig(), Configs.ToList());
                if (editConfigWindow.ShowDialog() == true)
                {
                    var newConfig = editConfigWindow.Config;
                    Configs.Add(newConfig);
                    _configManager.SaveConfig(Configs.Where(c => c.Name != DHCP_CONFIG_NAME).ToList());
                    _logger.Info($"成功添加配置: {newConfig.Name}");
                    _notificationService.ShowSuccess($"配置 '{newConfig.Name}' 添加成功");
                }
                else
                {
                    _logger.Info("用户取消添加配置");
                }
            }
            catch (Exception ex)
            {
                _logger.Error("添加配置失败: " + ex.Message);
                await _dialogService.ShowErrorAsync($"添加配置失败: {ex.Message}");
            }
        }

        private async Task EditConfigAsync()
        {
            if (SelectedConfig == null) return;

            try
            {
                _logger.Info($"开始编辑配置: {SelectedConfig.Name}");

                if (IsDhcpConfig(SelectedConfig))
                {
                    _logger.Info("尝试编辑DHCP配置，已拒绝");
                    await _dialogService.ShowWarningAsync("DHCP配置项不能被编辑");
                    return;
                }

                var index = Configs.IndexOf(SelectedConfig);
                if (index >= 0)
                {
                    var editConfigWindow = new EditConfigWindow(SelectedConfig, Configs.ToList());
                    if (editConfigWindow.ShowDialog() == true)
                    {
                        var updatedConfig = editConfigWindow.Config;
                        Configs[index] = null;
                        Configs[index] = updatedConfig;
                        _configManager.SaveConfig(Configs.Where(c => c.Name != DHCP_CONFIG_NAME).ToList());
                        _logger.Info($"成功编辑配置: {updatedConfig.Name}");
                        _notificationService.ShowSuccess($"配置 '{updatedConfig.Name}' 更新成功");
                    }
                    else
                    {
                        _logger.Info("用户取消编辑配置");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("编辑配置失败: " + ex.Message);
                await _dialogService.ShowErrorAsync($"编辑配置失败: {ex.Message}");
            }
        }

        private async Task DeleteConfigAsync()
        {
            if (SelectedConfig == null) return;

            try
            {
                _logger.Info($"开始删除配置: {SelectedConfig.Name}");

                if (IsDhcpConfig(SelectedConfig))
                {
                    _logger.Info("尝试删除DHCP配置，已拒绝");
                    await _dialogService.ShowWarningAsync("DHCP配置项不能被删除");
                    return;
                }

                var confirmed = await _dialogService.ShowConfirmationAsync("确认删除", $"确定要删除配置 '{SelectedConfig.Name}' 吗?");
                if (confirmed)
                {
                    var configName = SelectedConfig.Name;
                    Configs.Remove(SelectedConfig);
                    _configManager.SaveConfig(Configs.Where(c => c.Name != DHCP_CONFIG_NAME).ToList());
                    _logger.Info($"成功删除配置: {configName}");
                    _notificationService.ShowSuccess($"配置 '{configName}' 已删除");
                }
                else
                {
                    _logger.Info("用户取消删除配置");
                }
            }
            catch (Exception ex)
            {
                _logger.Error("删除配置失败: " + ex.Message);
                await _dialogService.ShowErrorAsync($"删除配置失败: {ex.Message}");
            }
        }

        private async Task ApplyConfigAsync()
        {
            if (string.IsNullOrEmpty(SelectedNic) || SelectedConfig == null) return;

            try
            {
                IsBusy = true;
                StatusMessage = "正在应用配置";
                _logger.Info($"开始应用配置 '{SelectedConfig.Name}' 到网卡 '{SelectedNic}'");

                var confirmed = await _dialogService.ShowConfirmationAsync("确认应用", 
                    $"确定要将配置 '{SelectedConfig.Name}' 应用到网卡 '{SelectedNic}' 吗?");

                if (!confirmed)
                {
                    _logger.Info("用户取消应用配置");
                    return;
                }

                StatusMessage = "正在获取当前配置";
                var originalConfig = await Task.Run(() => _networkManager.GetCurrentIpConfig(SelectedNic));

                bool result;
                NetworkConfig appliedConfig;

                if (IsDhcpConfig(SelectedConfig))
                {
                    var dhcpConfig = new NetworkConfig { NicName = SelectedNic };
                    appliedConfig = dhcpConfig;
                    StatusMessage = "正在设置DHCP";
                    _logger.Info($"正在为网卡 '{SelectedNic}' 设置DHCP");
                    result = await _networkManager.SetIpConfigAsync(SelectedNic, dhcpConfig);
                }
                else
                {
                    SelectedConfig.NicName = SelectedNic;
                    appliedConfig = SelectedConfig;
                    StatusMessage = "正在设置静态IP";
                    _logger.Info($"正在为网卡 '{SelectedNic}' 设置静态IP: {SelectedConfig.IPAddress}");
                    result = await _networkManager.SetIpConfigAsync(SelectedNic, SelectedConfig);
                }

                if (result)
                {
                    _logger.Info($"配置 '{SelectedConfig.Name}' 应用到网卡 '{SelectedNic}' 成功");

                    var confirmDialog = new ConfirmDialog(_networkManager, SelectedNic, originalConfig, appliedConfig);
                    var dialogResult = confirmDialog.ShowDialog();

                    if (dialogResult == true)
                    {
                        _logger.Info($"用户确认了配置 '{SelectedConfig.Name}'");
                        _notificationService.ShowSuccess("配置应用成功并已确认");
                    }
                    else
                    {
                        _logger.Info($"配置 '{SelectedConfig.Name}' 未被确认，已自动还原");
                        _notificationService.ShowWarning("配置已还原");
                    }

                    var currentConfig = await Task.Run(() => _networkManager.GetCurrentIpConfig(SelectedNic));
                    UpdateCurrentConfigDisplay(currentConfig);
                }
                else
                {
                    _logger.Error($"配置 '{SelectedConfig.Name}' 应用到网卡 '{SelectedNic}' 失败");
                    await _dialogService.ShowErrorAsync("配置应用失败，可能需要管理员权限");
                }
            }
            catch (Exception ex)
            {
                _logger.Error("应用配置失败: " + ex.Message);
                await _dialogService.ShowErrorAsync($"应用配置失败: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void Exit()
        {
            Application.Current.MainWindow?.Close();
        }

        private bool CanEditConfig() => SelectedConfig != null && !IsDhcpConfig(SelectedConfig);
        private bool CanDeleteConfig() => SelectedConfig != null && !IsDhcpConfig(SelectedConfig);
        private bool CanApplyConfig() => !string.IsNullOrEmpty(SelectedNic) && SelectedConfig != null && !IsBusy;

        private bool IsDhcpConfig(NetworkConfig config) => config != null && config.Name == DHCP_CONFIG_NAME;
    }
}
