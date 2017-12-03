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
    public class NetworkMessageWriter:IDisposable
    {
        public TcpClient TcpClient { private set; get; }
        private NetworkStream netStream;

        public delegate void WriteCompletedHandler(object obj, NetworkMessageWriterWriteCompletedEventArgs e);
        public delegate void WriteErrorHandler(object obj, NetworkMessageWriterWriteErrorEventArgs e);

        public event WriteCompletedHandler WriteCompleted;
        public event WriteErrorHandler WriteError;


        public NetworkMessageWriter(TcpClient tcpClient)
        {
            TcpClient = tcpClient;
        }
        
        public void WriteAsync(NetworkMessage netMessage)
        {
            try
            {
                byte[] buffer = new NetworkMessageFormatter<NetworkMessage>().Serialize(netMessage);

                TcpClient.SendBufferSize = buffer.Length;
                netStream = TcpClient.GetStream();
                netStream.BeginWrite(buffer, 0, buffer.Length, writeCallback,netMessage);
            }
            catch (Exception ex)
            {
                WriteError?.BeginInvoke(this, new NetworkMessageWriterWriteErrorEventArgs(netMessage,TcpClient,ex),null,null);
            }
        }

        private void writeCallback(IAsyncResult ar)
        {
            try
            {
                netStream.EndWrite(ar);
                WriteCompleted?.BeginInvoke(this, new NetworkMessageWriterWriteCompletedEventArgs(ar.AsyncState as NetworkMessage,TcpClient),null,null);
            }
            catch (Exception ex)
            {
                WriteError?.BeginInvoke(this, new NetworkMessageWriterWriteErrorEventArgs(ar.AsyncState as NetworkMessage, TcpClient, ex),null,null);
            }
        }

        public void Dispose()
        {
            TcpClient?.Close();
            netStream?.Close();
            netStream?.Dispose();
        }
    }
}
