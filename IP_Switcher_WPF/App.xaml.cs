using System;
using System.Windows;
using System.Security.Principal;
using IP_Switcher;
using IP_Switcher.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace IP_Switcher_WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// 依赖注入容器
        /// </summary>
        public IServiceProvider ServiceProvider { get; private set; }

        /// <summary>
        /// 应用程序启动事件
        /// </summary>
        /// <param name="e">启动事件参数</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 检查是否以管理员身份运行
            if (!IsRunningAsAdministrator())
            {
                MessageBox.Show("此应用程序需要管理员权限才能运行，请右键点击应用程序图标并选择\"以管理员身份运行\"。", "权限不足", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }

            // 配置依赖注入容器
            var services = new ServiceCollection();
            ConfigureServices(services);

            ServiceProvider = services.BuildServiceProvider();

            // 显示主窗口
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        /// <summary>
        /// 检查应用程序是否以管理员身份运行
        /// </summary>
        /// <returns>如果以管理员身份运行则返回true，否则返回false</returns>
        private bool IsRunningAsAdministrator()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        /// <summary>
        /// 配置依赖注入服务
        /// </summary>
        /// <param name="services">服务集合</param>
        private void ConfigureServices(IServiceCollection services)
        {
            // 注册窗口
            services.AddTransient<MainWindow>();
            services.AddTransient<EditConfigWindow>();
            services.AddTransient<ConfirmDialog>();

            // 注册服务
            services.AddSingleton<INetworkManager, NetworkManager>();
            services.AddSingleton<IConfigManager, ConfigManager>();
            services.AddSingleton<ILogger, Logger>();
        }
    }
}

