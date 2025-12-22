using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace IP_Switcher.Models
{
    /// <summary>
    /// 网络配置模型
    /// </summary>
    [DataContract]
    public class NetworkConfig
    {
        /// <summary>
        /// 配置名称
        /// </summary>
        [DataMember(Name = "Name")]
        public string Name { get; set; }

        /// <summary>
        /// 网卡名称
        /// </summary>
        [DataMember(Name = "NicName")]
        public string NicName { get; set; }

        /// <summary>
        /// IP地址
        /// </summary>
        [DataMember(Name = "IPAddress")]
        public string IPAddress { get; set; }

        /// <summary>
        /// 子网掩码
        /// </summary>
        [DataMember(Name = "SubnetMask")]
        public string SubnetMask { get; set; }

        /// <summary>
        /// 默认网关
        /// </summary>
        [DataMember(Name = "DefaultGateway")]
        public string DefaultGateway { get; set; }

        /// <summary>
        /// DNS服务器列表
        /// </summary>
        [DataMember(Name = "DnsServers")]
        public List<string> DnsServers { get; set; }

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