using Network;
using System;

namespace VoiceBroadcastClient.Classes
{
    class ClientConnectedEventArgs :EventArgs
    {
        public BroadcastClient BroadcastClient { get; private set; }
        public ClientConnectedEventArgs(BroadcastClient broadcastClient)
        {
            BroadcastClient = broadcastClient;
        }
    }
}
