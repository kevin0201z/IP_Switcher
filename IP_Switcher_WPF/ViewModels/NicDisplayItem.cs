using System.Net.NetworkInformation;
using CommunityToolkit.Mvvm.ComponentModel;
using IP_Switcher.Models;

namespace IP_Switcher_WPF.ViewModels
{
    public partial class NicDisplayItem : ObservableObject
    {
        public string Name { get; }

        public string Type { get; }

        public NetworkInterfaceType InterfaceType { get; }

        public OperationalStatus Status { get; }

        public string IPAddress { get; }

        public string Description { get; }

        public string MacAddress { get; }

        [ObservableProperty]
        private string _iconPath;

        [ObservableProperty]
        private string _statusColor;

        [ObservableProperty]
        private string _typeIcon;

        public NicDisplayItem(NicInfo nicInfo)
        {
            Name = nicInfo.Name;
            Type = nicInfo.Type;
            InterfaceType = nicInfo.InterfaceType;
            Status = nicInfo.Status;
            IPAddress = nicInfo.IPAddress;
            Description = nicInfo.Description;
            MacAddress = nicInfo.MacAddress;

            SetIconAndColor();
        }

        private void SetIconAndColor()
        {
            StatusColor = Status == OperationalStatus.Up ? "#27ae60" : "#e74c3c";

            TypeIcon = GetTypeIcon(InterfaceType);
        }

        private string GetTypeIcon(NetworkInterfaceType type)
        {
            int typeValue = (int)type;
            
            // 首先检查 IANA ifType 值
            switch (typeValue)
            {
                case 6:   // Ethernet
                case 26:  // Ethernet3Megabit
                case 62:  // FastEthernetT
                case 69:  // FastEthernetFx
                case 117: // GigabitEthernet
                    return "🔌";
                    
                case 71:  // Wireless80211
                    return "📶";
                    
                case 53:  // PropVirtual - 虚拟网卡
                    return "💻";
                    
                case 131: // Tunnel
                case 114: // IPOverAtm
                    return "🔒";
                    
                case 23:  // Ppp
                case 20:  // BasicIsdn
                case 21:  // PrimaryIsdn
                case 63:  // Isdn
                    return "📡";
                    
                case 24:  // Loopback
                    return "🔄";
            }
            
            // 回退到枚举判断
            switch (type)
            {
                case NetworkInterfaceType.Wireless80211:
                    return "📶";
                    
                case NetworkInterfaceType.Ethernet:
                case NetworkInterfaceType.Ethernet3Megabit:
                case NetworkInterfaceType.FastEthernetT:
                case NetworkInterfaceType.FastEthernetFx:
                case NetworkInterfaceType.GigabitEthernet:
                    return "🔌";
                    
                case NetworkInterfaceType.Tunnel:
                case NetworkInterfaceType.IPOverAtm:
                    return "🔒";
                    
                case NetworkInterfaceType.Ppp:
                case NetworkInterfaceType.BasicIsdn:
                case NetworkInterfaceType.PrimaryIsdn:
                case NetworkInterfaceType.Isdn:
                    return "📡";
                    
                case NetworkInterfaceType.Loopback:
                    return "🔄";
                    
                default:
                    return "🌐";
            }
        }

        public string DisplayName
        {
            get
            {
                var ipDisplay = string.IsNullOrEmpty(IPAddress) ? "无IP" : IPAddress;
                return $"{Name} ({ipDisplay})";
            }
        }

        public string DisplayInfo
        {
            get
            {
                var statusText = Status == OperationalStatus.Up ? "已连接" : "未连接";
                return $"{Type} | {statusText}";
            }
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
