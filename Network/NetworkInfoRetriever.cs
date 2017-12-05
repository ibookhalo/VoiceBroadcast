using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace Network
{
    public static class NetworkInfoRetriever
    {
        public static bool IsNetworkAdapterUp(IPAddress ipAddress)
        {
            var nic = GetNetworkAdapterByIP(ipAddress);
            if (nic != null)
            {
                if (nic.OperationalStatus != OperationalStatus.Up)
                {
                    // cable unplugged ?
                    return false;
                }
            }
            else
            {
                // nic is disable ? nic not found? ip address changed?
                return false;
            }
            return true;
        }

        private static NetworkInterface GetNetworkAdapterByIP(IPAddress ip)
        {
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    foreach (UnicastIPAddressInformation ipInfo in nic.GetIPProperties().UnicastAddresses)
                    {
                        if (ipInfo.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && ipInfo.Address.Equals(ip))
                        {
                            return nic;
                        }
                    }
                }
            }
            return null;
        }
    }
}
