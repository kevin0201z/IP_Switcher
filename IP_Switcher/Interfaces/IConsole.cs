using System;

namespace IP_Switcher
{
    /// <summary>
    /// 控制台接口，定义了控制台输入输出的操作
    /// </summary>
    public interface IConsole
    {
        /// <summary>
        /// 清除控制台
        /// </summary>
        void Clear();

        /// <summary>
        /// 写入一行文本到控制台
        /// </summary>
        /// <param name="value">要写入的文本</param>
        void WriteLine(string value);

        /// <summary>
        /// 写入文本到控制台
        /// </summary>
        /// <param name="value">要写入的文本</param>
        void Write(string value);

        /// <summary>
        /// 从控制台读取一行输入
        /// </summary>
        /// <returns>用户输入的文本</returns>
        string ReadLine();

        /// <summary>
        /// 从控制台读取一个按键
        /// </summary>
        /// <returns>用户按下的键</returns>
        ConsoleKeyInfo ReadKey();
    }
}