
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Network.Messaging;

namespace Network.EventArgs
{
    public class NetworkMessageWriterWriteErrorEventArgs:NetworkMessageErrorEventArgs
    {
        public NetworkMessage NetworkMessage { private set; get; }
        public NetworkMessageWriterWriteErrorEventArgs(NetworkMessage netMessage, TcpClient tcpClient, Exception ex):base(tcpClient,ex)
        {
            this.NetworkMessage = netMessage;
        }
    }
}
