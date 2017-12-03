using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Network.Messaging
{
    [Serializable]
    public class VoiceMessage:NetworkMessage
    {
        
        public byte[] Data { get; private set; }
        public BroadCastClient Sender { get; private set; }

        public VoiceMessage(BroadCastClient sender, byte[] data)
        {
            this.Sender = sender;
            this.Data = data;
        }
    }
}
