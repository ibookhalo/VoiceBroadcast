using  Network.EventArgs;
using  Network.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace  Network.Messaging
{
    public class NetworkMessageReader:NetworkMessage
    {
        private bool readLoop;
        public TcpClient TcpClient { private set; get; }
        private NetworkStream netStream;

        public delegate void ReadCompletedHandler(object obj, NetworkMessageReaderReadCompletedEventArgs  e);
        public delegate void ReadErrorHandler(object obj, NetworkMessageReaderReadErrorEventArgs e);

        public event ReadCompletedHandler ReadCompleted;
        public event ReadErrorHandler ReadError;

        public NetworkMessageReader(TcpClient tcpClient)
        {
            this.TcpClient = tcpClient;
        }

        public void Stop()
        {
            netStream?.Close();
        }
        public void ReadAsync(bool readLoop = false)
        {
            try
            {
                this.readLoop = readLoop;

                byte[] buffer = new byte[TcpClient.ReceiveBufferSize = NetworkMessage.MAX_DATA_SIZE_IN_BYTES];
                netStream = TcpClient.GetStream();
                netStream.BeginRead(buffer, 0, buffer.Length, readCallback,buffer);
            }
            catch (Exception ex)
            {
                ReadError?.Invoke(this, new NetworkMessageReaderReadErrorEventArgs(TcpClient, ex));
            }
        }

        private void readCallback(IAsyncResult ar)
        {
            try
            {
                netStream.EndRead(ar);
                NetworkMessage netMesasge = new NetworkMessageFormatter<NetworkMessage>().Deserialize(ar.AsyncState as byte[]);
                if (netMesasge!=null)
                    ReadCompleted?.Invoke(this, new NetworkMessageReaderReadCompletedEventArgs(netMesasge, TcpClient));
                else
                    ReadError?.Invoke(this, new NetworkMessageReaderReadErrorEventArgs(TcpClient,new ArgumentNullException("NetworkMessage is null")));


                if (readLoop)
                {
                    ReadAsync(readLoop);
                }
            }
            catch (Exception ex)
            {
                ReadError?.Invoke(this, new NetworkMessageReaderReadErrorEventArgs(TcpClient, ex));
            }

        }
    }
}
