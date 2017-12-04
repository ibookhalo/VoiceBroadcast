using Network.Messaging;
using System;
using System.Net.Sockets;

namespace Network.EventArgs
{
    public class NetworkMessageWriterWriteErrorEventArgs : NetworkMessageErrorEventArgs
    {
        public NetworkMessage NetworkMessage { private set; get; }
        public NetworkMessageWriterWriteErrorEventArgs(NetworkMessage netMessage, TcpClient tcpClient, Exception ex) : base(tcpClient, ex)
        {
            this.NetworkMessage = netMessage;
        }
    }
}