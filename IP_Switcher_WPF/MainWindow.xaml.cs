using System;
using System.Windows;
using System.Windows.Media.Animation;
using IP_Switcher;
using IP_Switcher.Interfaces;
using IP_Switcher_WPF.Services;
using IP_Switcher_WPF.ViewModels;

namespace IP_Switcher_WPF
{
    public partial class MainWindow : Window
    {
        private readonly ILogger _logger;
        private readonly INetworkManager _networkManager;

        public MainViewModel ViewModel { get; private set; }

        public MainWindow(
            INetworkManager networkManager,
            IConfigManager configManager,
            ILogger logger,
            IDialogService dialogService,
            INotificationService notificationService)
        {
            InitializeComponent();

            _networkManager = networkManager;
            _logger = logger;

            ViewModel = new MainViewModel(networkManager, configManager, logger, dialogService, notificationService);
            DataContext = ViewModel;

            _networkManager.ErrorOccurred += NetworkManager_ErrorOccurred;
            Closed += MainWindow_Closed;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var fadeInStoryboard = Resources["WindowFadeIn"] as Storyboard;
            fadeInStoryboard?.Begin(this);
        }

        private void NetworkManager_ErrorOccurred(object sender, NetworkErrorEventArgs e)
        {
            _logger.Error(e.ErrorMessage, e.Exception);
            MessageBox.Show($"操作失败: {e.ErrorMessage}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            _networkManager.ErrorOccurred -= NetworkManager_ErrorOccurred;
            Closed -= MainWindow_Closed;
        }
    }
}