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

        public bool StopWritingOnError { get; set; }

        public NetworkMessageWriter(TcpClient tcpClient)
        {
            TcpClient = tcpClient;
        }
        
        public void WriteAsync(NetworkMessage netMessage)
        {
            byte[] buffer = new NetworkMessageFormatter<NetworkMessage>().Serialize(netMessage);

            TcpClient.SendBufferSize = buffer.Length;
            netStream = TcpClient.GetStream();
            if (netStream.CanWrite)
            {
                netStream.BeginWrite(buffer, 0, buffer.Length, writeCallback, netMessage);
            }
            else
            {
                throw new Exception("NetworkStream can not write");
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

                if (StopWritingOnError)
                {
                    netStream?.Close();
                    TcpClient?.Close();

                    WriteCompleted = null;
                    WriteError = null;
                }
            }
        }
    }
}
