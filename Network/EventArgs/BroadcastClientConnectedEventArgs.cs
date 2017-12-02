
namespace Network.EventArgs
{
    public class BroadcastClientConnectedEventArgs: System.EventArgs
    {
        public BroadCastClient BroadcastClient { private set; get; }

        public BroadcastClientConnectedEventArgs(BroadCastClient broadCastClient)
        {
            this.BroadcastClient = broadCastClient;
        }
    }
}
