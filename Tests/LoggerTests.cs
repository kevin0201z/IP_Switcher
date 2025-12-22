using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IP_Switcher.Tests
{
    [TestClass]
    public class LoggerTests
    {
        [TestMethod]
        public void Info_ShouldWriteToLogFile()
        {
            var logger = new Logger();
            string message = $"unittest-info-{Guid.NewGuid()}";
            logger.Info(message);

            AssertLogContains(message, "INFO");
        }

        [TestMethod]
        public void Warning_ShouldWriteToLogFile()
        {
            var logger = new Logger();
            string message = $"unittest-warning-{Guid.NewGuid()}";
            logger.Warning(message);

            AssertLogContains(message, "WARNING");
        }

        [TestMethod]
        public void Error_ShouldWriteToLogFile()
        {
            var logger = new Logger();
            string message = $"unittest-error-{Guid.NewGuid()}";
            logger.Error(message);

            AssertLogContains(message, "ERROR");
        }

        [TestMethod]
        public void ErrorWithException_ShouldWriteDetailedError()
        {
            var logger = new Logger();
            string message = $"unittest-exception-{Guid.NewGuid()}";
            var exception = new InvalidOperationException("Test exception");
            
            logger.Error(message, exception);

            string logsDir = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
            if (!Directory.Exists(logsDir))
            {
                Directory.CreateDirectory(logsDir);
            }
            
            string expectedFile = Path.Combine(logsDir, $"IPSwitcher_{DateTime.Now:yyyyMMdd}.log");
            
            Assert.IsTrue(File.Exists(expectedFile), $"日志文件应存在: {expectedFile}");
            
            string content = File.ReadAllText(expectedFile);
            Assert.IsTrue(content.Contains(message), "日志内容应包含测试消息");
            Assert.IsTrue(content.Contains("Test exception"), "日志内容应包含异常消息");
        }

        [TestMethod]
        public void CleanOldLogs_ShouldRemoveOldLogFiles()
        {
            var logger = new Logger();
            
            // 创建一个旧日志文件
            string logsDir = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
            if (!Directory.Exists(logsDir))
            {
                Directory.CreateDirectory(logsDir);
            }
            
            string oldLogFile = Path.Combine(logsDir, $"IPSwitcher_{DateTime.Now.AddDays(-8):yyyyMMdd}.log");
            File.WriteAllText(oldLogFile, "Old log content");
            
            // 显式设置文件创建时间为8天前
            FileInfo fileInfo = new FileInfo(oldLogFile);
            fileInfo.CreationTime = DateTime.Now.AddDays(-8);
            
            // 调用清理方法
            logger.CleanOldLogs();
            
            // 验证旧日志文件已被删除
            Assert.IsFalse(File.Exists(oldLogFile), "旧日志文件应被清理");
        }

        private void AssertLogContains(string message, string level)
        {
            string logsDir = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
            if (!Directory.Exists(logsDir))
            {
                Directory.CreateDirectory(logsDir);
            }
            
            string expectedFile = Path.Combine(logsDir, $"IPSwitcher_{DateTime.Now:yyyyMMdd}.log");
            
            Assert.IsTrue(File.Exists(expectedFile), $"日志文件应存在: {expectedFile}");
            
            string content = File.ReadAllText(expectedFile);
            Assert.IsTrue(content.Contains(message), "日志内容应包含测试消息");
            Assert.IsTrue(content.Contains(level), $"日志内容应包含日志级别: {level}");
        }
    }
}
