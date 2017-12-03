using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Network;

namespace VoiceBroadcastServer
{
    class ServerBroadCastClient
    {
        public Network.BroadCastClient Client { get; private set; }
        public TcpClient TcpClient { get; private set; }
        public ServerBroadCastClient(Network.BroadCastClient client, TcpClient tcpClient)
        {
            this.Client = client;
            this.TcpClient = tcpClient;
        }
    }
}
