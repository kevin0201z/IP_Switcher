using System;
using System.Windows;
using System.Windows.Media.Animation;
using IP_Switcher;
using IP_Switcher.Interfaces;

namespace IP_Switcher_WPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
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
            _networkManager.ErrorOccurred += NetworkManager_ErrorOccurred;
            
            // 订阅窗口关闭事件，用于清理资源
            Closed += MainWindow_Closed;
        }
        
        /// <summary>
        /// 窗口加载完成事件
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 触发窗口淡入动画
            var fadeInStoryboard = Resources["WindowFadeIn"] as Storyboard;
            fadeInStoryboard?.Begin(this);
        }

        /// <summary>
        /// 网络管理器错误事件处理
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">错误事件参数</param>
        private void NetworkManager_ErrorOccurred(object sender, NetworkErrorEventArgs e)
        {
            // 记录错误日志
            _logger.Error(e.ErrorMessage, e.Exception);
            
            // 显示友好的错误提示
            MessageBox.Show($"操作失败: {e.ErrorMessage}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        
        /// <summary>
        /// 窗口关闭事件处理，用于清理资源
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        private void MainWindow_Closed(object sender, EventArgs e)
        {
            // 取消订阅错误事件，防止内存泄漏
            _networkManager.ErrorOccurred -= NetworkManager_ErrorOccurred;
            
            // 取消订阅关闭事件
            Closed -= MainWindow_Closed;
        }
    }
}