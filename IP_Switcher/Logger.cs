using System;
using System.IO;
using System.Text;
using IP_Switcher.Interfaces;

namespace IP_Switcher
{
    /// <summary>
    /// 日志级别
    /// </summary>
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }

    /// <summary>
    /// 日志类
    /// </summary>
    public class Logger : ILogger
    {
        private readonly string logDirectory = "Logs";
        private readonly object lockObject = new object();

        /// <summary>
        /// 构造函数
        /// </summary>
        public Logger()
        {
            // 创建日志目录
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
            
            // 清理旧日志（保留最近7天）
            CleanOldLogs();
        }

        /// <summary>
        /// 写入调试日志
        /// </summary>
        /// <param name="message">日志消息</param>
        public void Debug(string message)
        {
            WriteLog(LogLevel.Debug, message);
        }

        /// <summary>
        /// 写入信息日志
        /// </summary>
        /// <param name="message">日志消息</param>
        public void Info(string message)
        {
            WriteLog(LogLevel.Info, message);
        }

        /// <summary>
        /// 写入警告日志
        /// </summary>
        /// <param name="message">日志消息</param>
        public void Warning(string message)
        {
            WriteLog(LogLevel.Warning, message);
        }

        /// <summary>
        /// 写入错误日志
        /// </summary>
        /// <param name="message">日志消息</param>
        public void Error(string message)
        {
            WriteLog(LogLevel.Error, message);
        }

        /// <summary>
        /// 写入错误日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="ex">异常对象</param>
        public void Error(string message, Exception ex)
        {
            string fullMessage = $"{message}\n异常信息: {ex.Message}\n堆栈跟踪: {ex.StackTrace}";
            WriteLog(LogLevel.Error, fullMessage);
        }

        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <param name="message">日志消息</param>
        private void WriteLog(LogLevel level, string message)
        {
            if (level < LogLevel.Error)
            {
                return;
            }
            lock (lockObject)
            {
                try
                {
                    // 生成日志文件名
                    string fileName = string.Format("IPSwitcher_{0:yyyyMMdd}.log", DateTime.Now);
                    string logFilePath = Path.Combine(logDirectory, fileName);

                    // 生成日志内容
                    string logContent = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level.ToString().ToUpper()}] {message}";

                    // 写入日志文件
                    using (StreamWriter writer = new StreamWriter(logFilePath, true, Encoding.UTF8))
                    {
                        writer.WriteLine(logContent);
                    }

                    // 控制台输出
                    Console.WriteLine(logContent);
                }
                catch (Exception ex)
                {
                    // 如果写入日志失败，直接输出到控制台
                    Console.WriteLine($"写入日志失败: {ex.Message}");
                    Console.WriteLine($"原日志内容: [{level.ToString().ToUpper()}] {message}");
                }
            }
        }

        /// <summary>
        /// 清理旧日志文件（保留最近7天）
        /// </summary>
        public void CleanOldLogs()
        {
            try
            {
                string[] logFiles = Directory.GetFiles(logDirectory, "IPSwitcher_*.log");
                DateTime sevenDaysAgo = DateTime.Now.AddDays(-7);

                foreach (string file in logFiles)
                {
                    FileInfo fileInfo = new FileInfo(file);
                    if (fileInfo.CreationTime < sevenDaysAgo)
                    {
                        fileInfo.Delete();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"清理旧日志失败: {ex.Message}");
            }
        }
    }
}