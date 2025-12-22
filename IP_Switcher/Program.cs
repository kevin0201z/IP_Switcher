using System;

namespace IP_Switcher
{
    internal class Program
    {
        static void Main()
        {
            try
            {
                // 使用控制台界面
                ConsoleUI consoleUI = new ConsoleUI();
                consoleUI.ShowMainMenu();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"程序发生未处理异常: {ex.Message}");
                Console.WriteLine("按任意键退出...");
                Console.ReadKey();
            }
        }
    }
}