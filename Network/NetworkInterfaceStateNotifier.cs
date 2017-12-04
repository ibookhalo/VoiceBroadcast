using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;

namespace Network
{
    public class NetworkInterfaceStateNotifier
    {

        public delegate void NetworkInterfaceStateChanged(object obj, System.EventArgs e);
        public event NetworkInterfaceStateChanged NetworkInterfaceIsNotUpEvent;
        public int TimerInterval {get;private set;}
        private Timer nicStateCheckerTimer;
        private IPAddress IPAddress;

        public NetworkInterfaceStateNotifier(int timerIntervalIn_ms, IPAddress ipAddress)
        {
            TimerInterval = timerIntervalIn_ms;
            IPAddress = ipAddress;
            nicStateCheckerTimer = new Timer(adapterStateCheckerTimerCallback, null, timerIntervalIn_ms, timerIntervalIn_ms);
        }
        private void adapterStateCheckerTimerCallback(object state)
        {
            var nic = getNetworkAdapterByIP(IPAddress);
            if (nic != null)
            {
                if (nic.OperationalStatus != OperationalStatus.Up)
                {
                    // cable unplugged ?
                    NetworkInterfaceIsNotUpEvent?.BeginInvoke(this, new System.EventArgs(), null, null);    
                }
            }
            else
            {
                // nic is disable ?
                NetworkInterfaceIsNotUpEvent?.BeginInvoke(this, new System.EventArgs(), null, null);
            }
        }
        private NetworkInterface getNetworkAdapterByIP(IPAddress ip)
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
