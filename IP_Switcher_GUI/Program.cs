using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using IP_Switcher;
using IP_Switcher.Display.WinForm;

namespace IP_Switcher_GUI
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                // 初始化WinForm应用
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                
                // 创建所有依赖对象
                NetworkManager networkManager = new NetworkManager();
                ConfigManager configManager = new ConfigManager();
                Logger logger = new Logger();
                
                // 创建主表单
                MainForm mainForm = new MainForm();
                mainForm.SetNetworkManager(networkManager);
                mainForm.SetConfigManager(configManager);
                mainForm.SetLogger(logger);
                
                // 加载初始数据
                mainForm.LoadNicList();
                mainForm.LoadConfigList();
                
                // 运行应用
                Application.Run(mainForm);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"程序发生未处理异常: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }    
    }
}