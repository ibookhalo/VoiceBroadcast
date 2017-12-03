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
        public BroadCastClient BroadCastClient { get; private set; }
        public ConnectMessage(BroadCastClient client)
        {
            this.BroadCastClient = client;
        }
    }
}
