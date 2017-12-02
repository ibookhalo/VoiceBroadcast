using  Network.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Network.EventArgs
{
    public class NetworkMessageWriterWriteCompletedEventArgs: System.EventArgs
    {
        public TcpClient TcpClient { private set; get; }
        public NetworkMessage NetMessage { private set; get; }
        public NetworkMessageWriterWriteCompletedEventArgs(NetworkMessage netMessage, TcpClient tcpClient)
        {
            this.TcpClient = tcpClient;
            this.NetMessage = netMessage;
        }
    }
}
