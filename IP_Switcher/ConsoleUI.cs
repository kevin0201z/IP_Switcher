using System;
using System.Collections.Generic;
using IP_Switcher.Interfaces;
using IP_Switcher.Display;
using IP_Switcher.Models;

namespace IP_Switcher
{
    /// <summary>
    /// 控制台交互类，负责业务逻辑和显示层的协调
    /// </summary>
    public class ConsoleUI
    {
        private readonly INetworkManager networkManager;
        private readonly IConfigManager configManager;
        private readonly ILogger logger;
        private readonly IDisplayLayer displayLayer;

        /// <summary>
        /// 构造函数
        /// </summary>
        public ConsoleUI()
            : this(new NetworkManager(), new ConfigManager(), new Logger(), new ConsoleDisplayLayer())
        {
        }

        /// <summary>
        /// 支持依赖注入的构造函数（便于单元测试）
        /// </summary>
        public ConsoleUI(INetworkManager networkManager, IConfigManager configManager, ILogger logger, IDisplayLayer displayLayer)
        {
            this.networkManager = networkManager ?? new NetworkManager();
            this.configManager = configManager ?? new ConfigManager();
            this.logger = logger ?? new Logger();
            this.displayLayer = displayLayer ?? new ConsoleDisplayLayer();
        }

        /// <summary>
        /// 显示主菜单
        /// </summary>
        public void ShowMainMenu()
        {
            try
            {
                logger.Info("程序启动");

                // 显示欢迎界面
                displayLayer.ShowWelcome();

                // 扫描网卡
                List<NicInfo> nicList = networkManager.GetAllNetworkAdapters() ?? new List<NicInfo>();
                logger.Info($"扫描到{nicList.Count}个可用网卡");

                // 显示网卡列表并让用户选择
                string defaultNicName = configManager.GetLastNic();
                NicInfo selectedNic = displayLayer.SelectNetworkAdapter(nicList, defaultNicName);
                if (selectedNic == null)
                {
                    logger.Error("用户选择了无效网卡");
                    return;
                }

                // 保存默认网卡
                configManager.SetLastNic(selectedNic.Name);

                // 加载配置文件
                List<NetworkConfig> configs = configManager.LoadConfig() ?? new List<NetworkConfig>();
                logger.Info($"加载到{configs.Count}个配置方案");

                // 显示配置方案并让用户选择
                NetworkConfig selectedConfig = displayLayer.SelectNetworkConfig(configs);
                if (selectedConfig == null)
                {
                    logger.Error("用户选择了无效配置方案");
                    return;
                }

                // 确保配置的网卡名称与选择的网卡一致
                selectedConfig.NicName = selectedNic.Name;

                // 显示当前配置
                NetworkConfig currentConfig = networkManager.GetCurrentIpConfig(selectedNic.Name);
                displayLayer.ShowCurrentConfig(selectedNic.Name, currentConfig);

                // 显示配置比较和确认界面
                bool confirm = displayLayer.ShowConfigComparison(selectedNic, currentConfig, selectedConfig);
                if (!confirm)
                {
                    logger.Info("用户取消了配置切换操作");
                    return;
                }

                // 执行IP配置切换
                bool success = networkManager.SetIpConfig(selectedNic.Name, selectedConfig);
                
                // 显示操作结果
                displayLayer.ShowApplicationResult(success, selectedNic.Name, selectedConfig.Name);
                
                if (success)
                {
                    logger.Info($"成功将网卡{selectedNic.Name}的配置切换为{selectedConfig.Name}");
                    // 显示新的配置
                    NetworkConfig newConfig = networkManager.GetCurrentIpConfig(selectedNic.Name);
                    displayLayer.ShowCurrentConfig(selectedNic.Name, newConfig);
                }
                else
                {
                    logger.Error($"将网卡{selectedNic.Name}的配置切换为{selectedConfig.Name}失败");
                }
            }
            catch (Exception ex)
            {
                logger.Error("程序执行出错", ex);
                displayLayer.ShowError(ex.Message);
            }
            finally
            {
                logger.Info("程序退出");
                displayLayer.WaitForKeyPress();
            }
        }
    }
}