using System.Collections.Generic;
using System.ComponentModel;

namespace IP_Switcher.Models
{
    /// <summary>
    /// 网络配置模型
    /// </summary>
    public class NetworkConfig : INotifyPropertyChanged
    {
        private string _name;
        private string _nicName;
        private string _ipAddress;
        private string _subnetMask;
        private string _defaultGateway;
        private List<string> _dnsServers;

        /// <summary>
        /// 属性变化事件
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 触发属性变化事件
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 配置名称
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        /// <summary>
        /// 网卡名称
        /// </summary>
        public string NicName
        {
            get { return _nicName; }
            set
            {
                if (_nicName != value)
                {
                    _nicName = value;
                    OnPropertyChanged(nameof(NicName));
                }
            }
        }

        /// <summary>
        /// IP地址
        /// </summary>
        public string IPAddress
        {
            get { return _ipAddress; }
            set
            {
                if (_ipAddress != value)
                {
                    _ipAddress = value;
                    OnPropertyChanged(nameof(IPAddress));
                }
            }
        }

        /// <summary>
        /// 子网掩码
        /// </summary>
        public string SubnetMask
        {
            get { return _subnetMask; }
            set
            {
                if (_subnetMask != value)
                {
                    _subnetMask = value;
                    OnPropertyChanged(nameof(SubnetMask));
                }
            }
        }

        /// <summary>
        /// 默认网关
        /// </summary>
        public string DefaultGateway
        {
            get { return _defaultGateway; }
            set
            {
                if (_defaultGateway != value)
                {
                    _defaultGateway = value;
                    OnPropertyChanged(nameof(DefaultGateway));
                }
            }
        }

        /// <summary>
        /// DNS服务器列表
        /// </summary>
        public List<string> DnsServers
        {
            get { return _dnsServers; }
            set
            {
                if (_dnsServers != value)
                {
                    _dnsServers = value;
                    OnPropertyChanged(nameof(DnsServers));
                    OnPropertyChanged(nameof(DnsDisplayString));
                }
            }
        }

        /// <summary>
        /// 用于UI显示的DNS服务器字符串
        /// </summary>
        public string DnsDisplayString
        {
            get
            {
                if (DnsServers == null || DnsServers.Count == 0)
                {
                    return "";
                }
                return string.Join("; ", DnsServers);
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public NetworkConfig()
        {
            DnsServers = new List<string>();
        }
    }
}