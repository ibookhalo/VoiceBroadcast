using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Network;

namespace VoiceBroadcastServer
{
    class ServerBroadcastClient
    {
        public Network.BroadcastClient Client { get; private set; }
        public TcpClient TcpClient { get; private set; }
        public ServerBroadcastClient(Network.BroadcastClient client, TcpClient tcpClient)
        {
            this.Client = client;
            this.TcpClient = tcpClient;
        }
        public override string ToString()
        {
            return Client?.ToString();
        }
    }
}
