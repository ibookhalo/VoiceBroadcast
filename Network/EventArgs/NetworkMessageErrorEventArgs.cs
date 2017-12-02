using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Network.EventArgs
{
    public abstract class NetworkMessageErrorEventArgs
    {
        public TcpClient TcpClient { private set; get; }
        public Exception Exception { private set; get; }
        public NetworkMessageErrorEventArgs(TcpClient tcpClient, Exception ex)
        {
            this.TcpClient = tcpClient;
            this.Exception = ex;
        }
    }
}
