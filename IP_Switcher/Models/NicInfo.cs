using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;

namespace IP_Switcher.Models
{
    public class NicInfo
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public NetworkInterfaceType InterfaceType { get; set; }

        public OperationalStatus Status { get; set; }

        public string Description { get; set; }

        public string MacAddress { get; set; }

        public string IPAddress { get; set; }

        public NicInfo()
        {
        }

        public NicInfo(NetworkInterface networkInterface)
        {
            Name = networkInterface.Name;
            InterfaceType = networkInterface.NetworkInterfaceType;
            //Type = GetFriendlyTypeName(InterfaceType);
            Type = GetInterfaceCategory(networkInterface);
            Status = networkInterface.OperationalStatus;
            Description = networkInterface.Description;
            MacAddress = networkInterface.GetPhysicalAddress().ToString();

            var ipProps = networkInterface.GetIPProperties();
            var unicast = ipProps.UnicastAddresses.FirstOrDefault(a =>
                a.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            IPAddress = unicast?.Address?.ToString() ?? "";
        }

        private string GetFriendlyTypeName(NetworkInterfaceType type)
        {
            // 处理未定义的枚举值（如 53）
            int typeValue = (int)type;

            // 首先检查是否为已知的 IANA ifType 值
            switch (typeValue)
            {
                case 1: return "其他类型";
                case 6: return "以太网";
                case 9: return "令牌环";
                case 15: return "FDDI";
                case 20: return "ISDN基本速率";
                case 21: return "ISDN主速率";
                case 23: return "PPP";
                case 24: return "回环";
                case 26: return "以太网3Mbps";
                case 28: return "SLIP";
                case 37: return "ATM";
                case 48: return "调制解调器";
                case 53: return "虚拟网卡";  // PropVirtual - 专有虚拟/内部组件
                case 62: return "快速以太网";
                case 63: return "ISDN/X.25";
                case 69: return "快速以太网FX";
                case 71: return "无线802.11";
                case 94: return "ADSL";
                case 95: return "RADSL";
                case 96: return "SDSL";
                case 97: return "VDSL";
                case 114: return "IP over ATM";
                case 117: return "千兆以太网";
                case 131: return "隧道/VPN";
                case 143: return "多速率SDSL";
                case 144: return "IEEE 1394";
                case 237: return "WiMax";
                case 243: return "移动宽带(GSM)";
                case 244: return "移动宽带(CDMA)";
            }

            // 回退到枚举名称
            switch (type)
            {
                case NetworkInterfaceType.Ethernet:
                case NetworkInterfaceType.Ethernet3Megabit:
                case NetworkInterfaceType.FastEthernetT:
                case NetworkInterfaceType.FastEthernetFx:
                case NetworkInterfaceType.GigabitEthernet:
                    return "以太网";
                case NetworkInterfaceType.Wireless80211:
                    return "无线网络";
                case NetworkInterfaceType.Tunnel:
                case NetworkInterfaceType.IPOverAtm:
                    return "VPN/隧道";
                case NetworkInterfaceType.Ppp:
                case NetworkInterfaceType.BasicIsdn:
                case NetworkInterfaceType.PrimaryIsdn:
                case NetworkInterfaceType.Isdn:
                    return "拨号连接";
                case NetworkInterfaceType.Loopback:
                    return "回环";
                case NetworkInterfaceType.Atm:
                    return "ATM";
                case NetworkInterfaceType.Fddi:
                    return "FDDI";
                case NetworkInterfaceType.TokenRing:
                    return "令牌环";
                default:
                    return $"网络适配器({typeValue})";
            }
        }
        public string GetInterfaceCategory(NetworkInterface ni)
        {
            int rawType = (int)ni.NetworkInterfaceType;
            string name = ni.Name.ToLower();
            string desc = ni.Description.ToLower();

            // 1. TAP / TUN / OpenVPN
            if (rawType == 53 ||
                name.Contains("tap") ||
                desc.Contains("tap") ||
                desc.Contains("vpn"))
            {
                return "TAP / VPN 虚拟网卡";
            }

            // 2. Hyper-V
            if (name.Contains("hyper-v") || desc.Contains("hyper-v"))
                return "Hyper-V 虚拟网卡";

            // 3. VMware
            if (name.Contains("vmware") || desc.Contains("vmware"))
                return "VMware 虚拟网卡";

            // 4. WSL
            if (name.Contains("wsl") || desc.Contains("wsl"))
                return "WSL 虚拟网卡";

            // 5. VirtualBox
            if (name.Contains("virtualbox") || desc.Contains("virtualbox"))
                return "VirtualBox 虚拟网卡";

            // 6. Microsoft Bridge / Virtual Switch
            if (desc.Contains("virtual") || desc.Contains("switch") || desc.Contains("bridge"))
                return "Windows 虚拟交换机 / 桥接网卡";

            // 7. 真实网卡（Ethernet / WiFi）
            if (ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
                ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
            {
                return "物理网卡";
            }

            // 8. 其他未知类型
            return $"未知类型（原始值: {rawType}）";
        }
    }
}