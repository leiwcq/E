using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using E.Configurations;

namespace E.Host.Services
{
    internal class ServiceConfig
    {
        public string ClusterId { get; set; }
        public string HostName { get; set; }

        public string Invariant { get; set; }
        public string OrleansConnectionString { get; set; }

        public int SiloPort { get; set; }

        public int GatewayPort { get; set; }

        public int DashboardPort { get; set; }
    }

    public static class ServiceContext
    {
        

        private static AssemblyConfiguration<ServiceConfig> _configuration;

        internal static AssemblyConfiguration<ServiceConfig> Configuration =>
            _configuration ?? (_configuration = new AssemblyConfiguration<ServiceConfig>("ServiceConfig.json",
                new ServiceConfig
                {
                    ClusterId = "E.Host.Services",
                    HostName = "192.168.1.162",
                    Invariant = "MySql.Data.MySqlClient",
                    OrleansConnectionString = "server=localhost;database=data;user=root;password=123456;SslMode=none;",
                    SiloPort = 22222,
                    GatewayPort = 40000,
                    DashboardPort = 38000
                }));

        /// <summary>
        /// ClusterId
        /// </summary>
        public static string ClusterId => Configuration.Root.ClusterId;

        /// <summary>
        /// 主机名
        /// </summary>
        public static string HostName => Configuration.Root.HostName;

        /// <summary>
        /// Orleans存储提供器
        /// </summary>
        public static string Invariant => Configuration.Root.Invariant;

        /// <summary>
        /// Orleans连接字符串
        /// </summary>
        public static string OrleansConnectionString => Configuration.Root.OrleansConnectionString;

        /// <summary>
        /// Silo端口
        /// </summary>
        public static int SiloPort => Configuration.Root.SiloPort;

        /// <summary>
        /// Gateway端口
        /// </summary>
        public static int GatewayPort => Configuration.Root.GatewayPort;

        /// <summary>
        /// Dashboard端口
        /// </summary>
        public static int DashboardPort => Configuration.Root.DashboardPort;

        public static int GetRandAvailablePort()
        {
            const int minPortN = 20000;
            const int maxPortN = 30000;
            const int mid = (minPortN + 9 * maxPortN) / 10;
            var rand = new Random();
            var startPort = rand.Next(minPortN, mid);
            for (var i = startPort; i <= maxPortN; i++)
            {
                if (PortIsAvailable(i)) return i;
            }

            return -1;
        }

        // Get the used port list  
        public static List<int> PortIsUsed()
        {
            var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            var ipsTcp = ipGlobalProperties.GetActiveTcpListeners();
            var ipsUdp = ipGlobalProperties.GetActiveUdpListeners();
            var tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

            var allPorts = ipsTcp.Select(ep => ep.Port).ToList();
            allPorts.AddRange(ipsUdp.Select(ep => ep.Port));
            allPorts.AddRange(tcpConnInfoArray.Select(conn => conn.LocalEndPoint.Port));

            return allPorts;
        }

        // Check whether the port is in the used list  
        public static bool PortIsAvailable(int port)
        {
            var isAvailable = true;
            var portUsed = PortIsUsed();

            foreach (var p in portUsed)
            {
                if (p == port)
                {
                    isAvailable = false;
                    break;
                }
            }

            return isAvailable;
        }

        public class GatewayHandlerTypes
        {
            public string GatewayName { get; set; }
            public Type AccountHandlerType { get; set; }

            public virtual string AccountHandlerTypeName
            {
                get
                {
                    if (AccountHandlerType != null)
                    {
                        return AccountHandlerType.FullName;
                    }
                    return string.Empty;
                }
            }

            public Type AccountManageHandlerType { get; set; }

            public virtual string AccountManageHandlerTypeName
            {
                get
                {
                    if (AccountManageHandlerType != null)
                    {
                        return AccountManageHandlerType.FullName;
                    }
                    return string.Empty;
                }
            }
        }
    }
}
