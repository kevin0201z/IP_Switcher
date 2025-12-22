using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;

namespace IP_Switcher.Models
{
    /// <summary>
    /// 网卡信息模型
    /// </summary>
    public class NicInfo
    {
        /// <summary>
        /// 网卡名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 网卡类型
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 连接状态
        /// </summary>
        public OperationalStatus Status { get; set; }

        /// <summary>
        /// 网卡描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// MAC地址
        /// </summary>
        public string MacAddress { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public NicInfo()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="networkInterface">NetworkInterface对象</param>
        public NicInfo(NetworkInterface networkInterface)
        {
            Name = networkInterface.Name;
            Type = networkInterface.NetworkInterfaceType.ToString();
            Status = networkInterface.OperationalStatus;
            Description = networkInterface.Description;
            MacAddress = networkInterface.GetPhysicalAddress().ToString();
        }
    }
}