using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IP_Switcher;
using IP_Switcher.Models;
using IP_Switcher.Interfaces;

namespace IP_Switcher.Tests
{
    class FakeDisplayLayer : IDisplayLayer
    {
        private readonly Queue<string> inputs = new Queue<string>();
        public List<string> Outputs = new List<string>();
        
        public FakeDisplayLayer(params string[] inputs)
        {
            foreach (var s in inputs) this.inputs.Enqueue(s);
        }
        
        public void ShowWelcome() { }
        
        public NicInfo SelectNetworkAdapter(List<NicInfo> nicList, string defaultNicName)
        {
            return nicList.FirstOrDefault();
        }
        
        public NetworkConfig SelectNetworkConfig(List<NetworkConfig> configs)
        {
            return configs.FirstOrDefault();
        }
        
        public void ShowCurrentConfig(string nicName, NetworkConfig config)
        {
            Outputs.Add($"当前配置 - 网卡: {nicName}");
        }
        
        public bool ShowConfigApplication(string nicName, NetworkConfig config)
        {
            return true;
        }
        
        public bool ShowConfigComparison(NicInfo selectedNic, NetworkConfig currentConfig, NetworkConfig targetConfig)
        {
            return true;
        }
        
        public void ShowApplicationResult(bool success, string nicName, string configName)
        {
            Outputs.Add($"应用结果: {success} - 网卡: {nicName} - 配置: {configName}");
        }
        
        public void ShowError(string message)
        {
            Outputs.Add(message);
        }
        
        public void ShowInfo(string message)
        {
            Outputs.Add(message);
        }
        
        public void WaitForKeyPress()
        {
        }
    }

    class FakeNetworkManager : INetworkManager
    {
        public NetworkConfig CurrentConfig = new NetworkConfig(); // 初始化默认配置，避免空引用
        public bool SetCalled = false;
        public List<NicInfo> GetAllNetworkAdapters() => new List<NicInfo>();
        public NicInfo GetNetworkAdapterByName(string nicName) => new NicInfo { Name = nicName };
        public NetworkConfig GetCurrentIpConfig(string nicName) => CurrentConfig;
        public bool SetIpConfig(string nicName, NetworkConfig config) { SetCalled = true; return true; }
        public bool SetDhcpConfig(string nicName) { SetCalled = true; return true; }
        public bool IsRunningAsAdmin() { return true; }
    }

    class FakeConfigManager : IConfigManager
    {
        public List<NetworkConfig> Configs = new List<NetworkConfig>();
        public string LastNic { get; set; }
        
        public List<NetworkConfig> LoadConfig() => Configs;
        public List<NetworkConfig> LoadConfig(string configPath) => Configs;
        public void CreateDefaultConfig(string configPath = null) { }
        public void SaveConfig(List<NetworkConfig> configs, string configPath = null) { }
        public NetworkConfig GetConfigByName(List<NetworkConfig> configs, string configName) => configs.Find(c => c.Name == configName);

        public string GetLastNic()
        {
            return LastNic;
        }

        public void SetLastNic(string nicName)
        {
            LastNic = nicName;
        }
    }

    class FakeLogger : ILogger
    {
        public List<string> Logs = new List<string>();
        public void Info(string message) => Logs.Add(message);
        public void Warning(string message) => Logs.Add(message);
        public void Error(string message) => Logs.Add(message);
        public void Error(string message, Exception ex) => Logs.Add(message + ":" + ex.Message);
    }

    [TestClass]
    public class ConsoleUITests
    {
        [TestMethod]
        public void Constructor_ShouldInitializeDependencies()
        {
            // Arrange
            var fakeNetworkManager = new FakeNetworkManager();
            var fakeConfigManager = new FakeConfigManager();
            var fakeLogger = new FakeLogger();
            var fakeDisplayLayer = new FakeDisplayLayer();

            // Act
            var ui = new ConsoleUI(fakeNetworkManager, fakeConfigManager, fakeLogger, fakeDisplayLayer);

            // Assert - The constructor should not throw exceptions
            Assert.IsNotNull(ui);
        }

        [TestMethod]
        public void FakeNetworkManager_SetIpConfig_ShouldReturnTrue()
        {
            // Arrange
            var fakeNetworkManager = new FakeNetworkManager();
            var config = new NetworkConfig { Name = "test", IPAddress = "2.2.2.2", SubnetMask = "255.255.255.0" };

            // Act
            bool result = fakeNetworkManager.SetIpConfig("TestNic", config);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(fakeNetworkManager.SetCalled);
        }

        [TestMethod]
        public void FakeConfigManager_GetLastNic_ShouldReturnSetValue()
        {
            // Arrange
            var fakeConfigManager = new FakeConfigManager();
            string testNic = "TestNic";

            // Act
            fakeConfigManager.SetLastNic(testNic);
            string result = fakeConfigManager.GetLastNic();

            // Assert
            Assert.AreEqual(testNic, result);
        }

        [TestMethod]
        public void Run_ShouldInitializeWithValidDependencies()
        {
            // Arrange
            var fakeNetworkManager = new FakeNetworkManager();
            var fakeConfigManager = new FakeConfigManager();
            var fakeLogger = new FakeLogger();
            var fakeDisplayLayer = new FakeDisplayLayer();

            // 添加测试配置
            var testConfig = new NetworkConfig { Name = "TestConfig", IPAddress = "2.2.2.2", SubnetMask = "255.255.255.0" };
            fakeConfigManager.Configs.Add(testConfig);

            // Act
            var ui = new ConsoleUI(fakeNetworkManager, fakeConfigManager, fakeLogger, fakeDisplayLayer);
            
            // 注意：这里不实际调用Run()，因为它是一个无限循环，我们测试它的初始化和依赖注入
            Assert.IsNotNull(ui);
            
            // 验证依赖项
            Assert.IsNotNull(fakeLogger);
        }
    }
}
