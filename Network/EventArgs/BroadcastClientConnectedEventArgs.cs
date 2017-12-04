
namespace Network.EventArgs
{
    public class BroadcastClientConnectedEventArgs: System.EventArgs
    {
        public BroadcastClient BroadcastClient { private set; get; }

        public BroadcastClientConnectedEventArgs(BroadcastClient broadCastClient)
        {
            this.BroadcastClient = broadCastClient;
        }
    }
}
