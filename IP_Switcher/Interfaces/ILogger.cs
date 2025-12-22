using System;

namespace IP_Switcher
{
    /// <summary>
    /// 日志接口，定义了日志记录的操作
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// 写入信息日志
        /// </summary>
        /// <param name="message">日志消息</param>
        void Info(string message);

        /// <summary>
        /// 写入警告日志
        /// </summary>
        /// <param name="message">日志消息</param>
        void Warning(string message);

        /// <summary>
        /// 写入错误日志
        /// </summary>
        /// <param name="message">日志消息</param>
        void Error(string message);

        /// <summary>
        /// 写入错误日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="ex">异常对象</param>
        void Error(string message, Exception ex);
    }
}