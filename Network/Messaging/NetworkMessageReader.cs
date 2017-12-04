using  Network.EventArgs;
using  Network.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Network.Messaging
{
    public class NetworkMessageReader
    {
        private bool readLoop;
        public TcpClient TcpClient { private set; get; }
        private NetworkStream netStream;

        public delegate void ReadCompletedHandler(object obj, NetworkMessageReaderReadCompletedEventArgs e);
        public delegate void ReadErrorHandler(object obj, NetworkMessageErrorEventArgs e);

        public event ReadCompletedHandler ReadCompleted;
        public event ReadErrorHandler ReadError;

        public bool OnErrorStopReadingAndCloseClient { get; set; }

        public NetworkMessageReader(TcpClient tcpClient)
        {
            TcpClient = tcpClient;
        }
        public void ReadAsync(bool readLoop = false)
        {
            this.readLoop = readLoop;
            TcpClient.ReceiveBufferSize = NetworkMessage.MAX_SIZE_BYTE;

            byte[] buffer = new byte[NetworkMessage.MAX_SIZE_BYTE];
            netStream = TcpClient.GetStream();
            if (netStream.CanRead)
            {
                netStream.BeginRead(buffer, 0, buffer.Length, readCallback, buffer);
            }
            else
            {
               throw  new Exception("NetworkStream can not read");
            }
        }

        private void readCallback(IAsyncResult ar)
        {
            try
            {
                netStream.EndRead(ar);
                NetworkMessage netMesasge = new NetworkMessageFormatter<NetworkMessage>().Deserialize(ar.AsyncState as byte[]);
                if (netMesasge!=null)
                    ReadCompleted?.BeginInvoke(this, new NetworkMessageReaderReadCompletedEventArgs(netMesasge, TcpClient),null,null);
                else
                    ReadError?.BeginInvoke(this, new NetworkMessageErrorEventArgs(TcpClient,new ArgumentNullException("NetworkMessage is null")),null,null);

                if (readLoop)
                {
                    ReadAsync(readLoop);
                }
            }
            catch (Exception ex)
            {
                ReadError?.BeginInvoke(this, new NetworkMessageErrorEventArgs(TcpClient, ex),null,null);

                if (OnErrorStopReadingAndCloseClient)
                {
                    netStream?.Close();
                    TcpClient?.Close();

                    ReadCompleted = null;
                    ReadError = null;
                }
            }

        }
    }
}
