using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IP_Switcher;
using IP_Switcher.Models;
using System.Collections.Generic;

namespace IP_Switcher.Tests
{
    [TestClass]
    public class ConfigManagerTests
    {
        [TestMethod]
        public void CreateDefaultConfig_ShouldCreateValidConfigFile()
        {
            string tempPath = Path.Combine(Path.GetTempPath(), $"ip_switcher_test_{Guid.NewGuid()}.json");
            try
            {
                var cm = new ConfigManager();
                cm.CreateDefaultConfig(tempPath);

                Assert.IsTrue(File.Exists(tempPath), "配置文件应被创建");
            }
            finally
            {
                if (File.Exists(tempPath)) File.Delete(tempPath);
            }
        }

        [TestMethod]
        public void LoadConfig_ShouldReturnValidConfigs()
        {
            string tempPath = Path.Combine(Path.GetTempPath(), $"ip_switcher_test_{Guid.NewGuid()}.json");
            try
            {
                var cm = new ConfigManager();
                cm.CreateDefaultConfig(tempPath);

                List<NetworkConfig> configs = cm.LoadConfig(tempPath);

                Assert.IsNotNull(configs, "返回的配置列表不应为空");
                Assert.IsTrue(configs.Count >= 1, "默认配置应至少包含一个 NetworkConfig");
                foreach (var c in configs)
                {
                    Assert.IsFalse(string.IsNullOrWhiteSpace(c.Name), "配置名称不应为空");
                }
            }
            finally
            {
                if (File.Exists(tempPath)) File.Delete(tempPath);
            }
        }

        [TestMethod]
        public void SaveConfig_ShouldPersistConfigs()
        {
            string tempPath = Path.Combine(Path.GetTempPath(), $"ip_switcher_test_{Guid.NewGuid()}.json");
            try
            {
                var cm = new ConfigManager();
                
                // 创建测试配置
                var testConfigs = new List<NetworkConfig>
                {
                    new NetworkConfig { Name = "TestConfig", IPAddress = "192.168.1.100", SubnetMask = "255.255.255.0", DefaultGateway = "192.168.1.1" }
                };
                
                // 保存配置
                cm.SaveConfig(testConfigs, tempPath);
                
                // 重新加载并验证
                List<NetworkConfig> loadedConfigs = cm.LoadConfig(tempPath);
                
                Assert.IsNotNull(loadedConfigs);
                Assert.AreEqual(1, loadedConfigs.Count);
                Assert.AreEqual("TestConfig", loadedConfigs[0].Name);
                Assert.AreEqual("192.168.1.100", loadedConfigs[0].IPAddress);
            }
            finally
            {
                if (File.Exists(tempPath)) File.Delete(tempPath);
            }
        }

        [TestMethod]
        public void GetLastNic_ShouldReturnSavedValue()
        {
            string tempPath = Path.Combine(Path.GetTempPath(), $"ip_switcher_test_{Guid.NewGuid()}.json");
            try
            {
                var cm = new ConfigManager();
                const string testNic = "TestNetworkAdapter";
                
                // 设置并保存最后使用的网卡
                cm.SetLastNic(testNic);
                cm.SaveConfig(new List<NetworkConfig>(), tempPath);
                
                // 重新加载并验证
                cm = new ConfigManager();
                string lastNic = cm.GetLastNic();
                
                Assert.AreEqual(testNic, lastNic);
            }
            finally
            {
                if (File.Exists(tempPath)) File.Delete(tempPath);
            }
        }

        [TestMethod]
        public void SetLastNic_ShouldUpdateLastNic()
        {
            var cm = new ConfigManager();
            const string testNic = "TestNetworkAdapter";
            
            cm.SetLastNic(testNic);
            string lastNic = cm.GetLastNic();
            
            Assert.AreEqual(testNic, lastNic);
        }

        [TestMethod]
        public void LoadConfig_WithInvalidPath_ShouldCreateDefaultConfig()
        {
            string tempPath = Path.Combine(Path.GetTempPath(), $"ip_switcher_invalid_{Guid.NewGuid()}.json");
            try
            {
                var cm = new ConfigManager();
                List<NetworkConfig> configs = cm.LoadConfig(tempPath);
                
                Assert.IsNotNull(configs, "返回的配置列表不应为空");
                Assert.IsTrue(configs.Count >= 1, "无效路径应创建默认配置");
            }
            finally
            {
                if (File.Exists(tempPath)) File.Delete(tempPath);
            }
        }
    }
}
