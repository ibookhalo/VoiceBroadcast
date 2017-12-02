using System;
using System.Net.Sockets;


namespace Network.EventArgs
{
    public class NetworkMessageReaderReadErrorEventArgs:NetworkMessageErrorEventArgs
    {
        public NetworkMessageReaderReadErrorEventArgs(TcpClient tcpClient, Exception ex):base(tcpClient,ex) {}
    }
}
