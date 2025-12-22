using System;
using System.Configuration;
using System.Data;
using System.Windows;
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

            // 配置依赖注入容器
            var services = new ServiceCollection();
            ConfigureServices(services);

            ServiceProvider = services.BuildServiceProvider();

            // 显示主窗口
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
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

