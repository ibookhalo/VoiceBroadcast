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
    public class NetworkMessageWriter
    {
        public TcpClient TcpClient { private set; get; }
        private NetworkStream netStream;

        public delegate void WriteCompletedHandler(object obj, NetworkMessageWriterWriteCompletedEventArgs e);
        public delegate void WriteErrorHandler(object obj, NetworkMessageWriterWriteErrorEventArgs e);

        public event WriteCompletedHandler WriteCompleted;
        public event WriteErrorHandler WriteError;


        public NetworkMessageWriter(TcpClient tcpClient)
        {
            this.TcpClient = tcpClient;
        }

        public void Stop()
        {
            this.netStream?.Dispose();
        }
        public void WriteAsync(NetworkMessage netMessage)
        {
            try
            {
                TcpClient.SendBufferSize = NetworkMessage.MAX_DATA_SIZE_IN_BYTES;
                byte[] buffer = new NetworkMessageFormatter<NetworkMessage>().Serialize(netMessage);

                netStream = TcpClient.GetStream();
                netStream.BeginWrite(buffer, 0, buffer.Length, writeCallback,netMessage);
            }
            catch (Exception ex)
            {
                WriteError?.Invoke(this, new NetworkMessageWriterWriteErrorEventArgs(netMessage,TcpClient,ex));
            }
        }

        private void writeCallback(IAsyncResult ar)
        {
            try
            {
                netStream.EndWrite(ar);
                WriteCompleted?.Invoke(this, new NetworkMessageWriterWriteCompletedEventArgs(ar.AsyncState as NetworkMessage,TcpClient));
            }
            catch (Exception ex)
            {
                WriteError?.Invoke(this, new NetworkMessageWriterWriteErrorEventArgs(ar.AsyncState as NetworkMessage, TcpClient, ex));
            }
        }
    }
}
