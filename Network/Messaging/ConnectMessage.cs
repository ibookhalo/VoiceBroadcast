using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Network.Messaging
{
    [Serializable]
    public class ConnectMessage:NetworkMessage
    {
        public bool Connected { get; set; }
        public BroadcastClient BroadCastClient { get; set; }
        public ConnectMessage(BroadcastClient client)
        {
            this.BroadCastClient = client;
        }

        public override string ToString()
        {
            return BroadCastClient.ToString();
        }
    }
}
