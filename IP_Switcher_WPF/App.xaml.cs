using System;
using System.Windows;
using System.Security.Principal;
using IP_Switcher;
using IP_Switcher.Interfaces;
using IP_Switcher_WPF.Services;
using IP_Switcher_WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace IP_Switcher_WPF
{
    public partial class App : Application
    {
        public IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (!IsRunningAsAdministrator())
            {
                MessageBox.Show("此应用程序需要管理员权限才能运行，请右键点击应用程序图标并选择\"以管理员身份运行\"。", "权限不足", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }

            var services = new ServiceCollection();
            ConfigureServices(services);

            ServiceProvider = services.BuildServiceProvider();

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private bool IsRunningAsAdministrator()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<MainWindow>();
            services.AddTransient<EditConfigWindow>();
            services.AddTransient<ConfirmDialog>();

            services.AddSingleton<INetworkManager, NetworkManager>();
            services.AddSingleton<IConfigManager, ConfigManager>();
            services.AddSingleton<ILogger, Logger>();

            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<INotificationService, NotificationService>();

            services.AddTransient<MainViewModel>();
        }
    }
}

