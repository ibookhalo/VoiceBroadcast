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
        public int TimerIntervalInSec { get; private set; }
        private Timer nicStateCheckerTimer;
        private IPAddress IPAddress;

        public NetworkInterfaceStateNotifier(int timerIntervalInSec, IPAddress ipAddress)
        {
            TimerIntervalInSec = timerIntervalInSec;
            IPAddress = ipAddress;
        }

        public void Start()
        {
            nicStateCheckerTimer = new Timer(adapterStateCheckerTimerCallback, null, TimerIntervalInSec * 1000, TimerIntervalInSec * 1000);
        }

        public void Stop()
        {
            nicStateCheckerTimer.Dispose();
            nicStateCheckerTimer = null;
        }
        private void adapterStateCheckerTimerCallback(object state)
        {
            if (!NetworkInfoRetriever.IsNetworkAdapterUp(IPAddress))
            {
                NetworkInterfaceIsNotUpEvent?.BeginInvoke(this, new System.EventArgs(), null, null);
            }
        }      
    }

}
