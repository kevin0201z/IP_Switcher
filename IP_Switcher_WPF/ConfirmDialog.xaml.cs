using System;
using System.Windows;
using System.Windows.Threading;
using IP_Switcher;
using IP_Switcher.Models;

namespace IP_Switcher_WPF
{
    /// <summary>
    /// ConfirmDialog.xaml 的交互逻辑
    /// </summary>
    public partial class ConfirmDialog : Window
    {
        // 倒计时秒数
        private const int COUNTDOWN_SECONDS = 10;
        // 当前倒计时值
        private int _currentCountdown = COUNTDOWN_SECONDS;
        // 倒计时定时器
        private DispatcherTimer _countdownTimer;
        // 原始网络配置（用于还原）
        private NetworkConfig _originalConfig;
        // 新网络配置
        private NetworkConfig _newConfig;
        // 网络管理器
        private readonly INetworkManager _networkManager;
        // 网卡名称
        private string _nicName;

        /// <summary>
        /// 确认结果
        /// </summary>
        public bool IsConfirmed { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="networkManager">网络管理器实例</param>
        /// <param name="nicName">网卡名称</param>
        /// <param name="originalConfig">原始网络配置（用于还原）</param>
        /// <param name="newConfig">新网络配置</param>
        public ConfirmDialog(INetworkManager networkManager, string nicName, NetworkConfig originalConfig, NetworkConfig newConfig)
        {
            InitializeComponent();
            
            _networkManager = networkManager;
            _nicName = nicName;
            _originalConfig = originalConfig;
            _newConfig = newConfig;
            
            InitializeTimer();
            SetupEventHandlers();
        }

        /// <summary>
        /// 初始化倒计时定时器
        /// </summary>
        private void InitializeTimer()
        {
            _countdownTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _countdownTimer.Tick += CountdownTimer_Tick;
        }

        /// <summary>
        /// 设置事件处理程序
        /// </summary>
        private void SetupEventHandlers()
        {
            confirmButton.Click += ConfirmButton_Click;
            restoreButton.Click += RestoreButton_Click;
        }

        /// <summary>
        /// 窗口加载时启动定时器
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _countdownTimer.Start();
        }

        /// <summary>
        /// 倒计时定时器事件
        /// </summary>
        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            _currentCountdown--;
            countdownText.Text = _currentCountdown.ToString();
            
            if (_currentCountdown <= 0)
            {
                // 倒计时结束，自动还原
                _countdownTimer.Stop();
                RestoreConfig();
            }
        }

        /// <summary>
        /// 确认按钮点击事件
        /// </summary>
        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            _countdownTimer.Stop();
            IsConfirmed = true;
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// 还原按钮点击事件
        /// </summary>
        private void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            _countdownTimer.Stop();
            RestoreConfig();
        }

        /// <summary>
        /// 还原网络配置
        /// </summary>
        private void RestoreConfig()
        {
            try
            {
                _networkManager.SetIpConfig(_nicName, _originalConfig);
                IsConfirmed = false;
                DialogResult = false;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"还原配置失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}