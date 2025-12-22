using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using IP_Switcher;
using IP_Switcher.Interfaces;
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
        
        private readonly ILogger _logger;
        private readonly INetworkManager _networkManager;
        private readonly IConfigManager _configManager;
        
        /// <summary>
        /// 主视图模型
        /// </summary>
        public MainViewModel ViewModel { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="networkManager">网络管理器</param>
        /// <param name="configManager">配置管理器</param>
        /// <param name="logger">日志记录器</param>
        public MainWindow(INetworkManager networkManager, IConfigManager configManager, ILogger logger)
        {
            InitializeComponent();
            
            _networkManager = networkManager;
            _configManager = configManager;
            _logger = logger;
            
            // 创建视图模型
            ViewModel = new MainViewModel(networkManager, configManager, logger);
            DataContext = ViewModel;
            
            // 订阅错误事件
            if (networkManager is NetworkManager networkManagerImpl)
            {
                networkManagerImpl.ErrorOccurred += NetworkManager_ErrorOccurred;
            }
        }

        /// <summary>
        /// 网络管理器错误事件处理
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">错误事件参数</param>
        private void NetworkManager_ErrorOccurred(object sender, NetworkErrorEventArgs e)
        {
            // 显示友好的错误提示
            MessageBox.Show($"操作失败: {e.ErrorMessage}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}