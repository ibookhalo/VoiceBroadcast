using Network;
using Network.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace VoiceBroadcastClient.Classes
{
    class TcpBroadcastClient
    {
        private TcpClient tcpClient;
        private NetworkMessageReader messageReader;
        public TcpBroadcastClient()
        {
            tcpClient = new TcpClient();
            messageReader = new NetworkMessageReader(tcpClient);

            messageReader.ReadCompleted += MessageReader_ReadCompleted;
            messageReader.ReadError += MessageReader_ReadError;
        }

        private void MessageReader_ReadCompleted(object obj, Network.EventArgs.NetworkMessageReaderReadCompletedEventArgs e)
        {
            if (e.NetworkMessage is ConnectMessage)
            {
                handleConnectMessage(e.NetworkMessage as ConnectMessage, e.TcpClient);
            }
            else if (e.NetworkMessage is VoiceMessage)
            {
                handleVoiceMessage(e.NetworkMessage as VoiceMessage, e.TcpClient);
            }
        }

        private void handleVoiceMessage(VoiceMessage voiceMessage, TcpClient tcpClient)
        {
            
        }

        private void handleConnectMessage(ConnectMessage connectMessage, TcpClient tcpClient)
        {
            
        }

        private void MessageReader_ReadError(object obj, Network.EventArgs.NetworkMessageReaderReadErrorEventArgs e)
        {
            
        }

        public bool Connected
        {
            get
            {
                try
                {
                    if (tcpClient.Client != null && tcpClient.Client.Connected)
                    {
                        // Detect if client disconnected
                        if (tcpClient.Client.Poll(0, SelectMode.SelectRead))
                        {
                            byte[] buff = new byte[1];
                            if (tcpClient.Client.Receive(buff, SocketFlags.Peek) == 0)
                            {
                                // Client disconnected
                                return false;
                            }
                            else
                            {
                                return true;
                            }
                        }
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch
                {
                    return false;
                }
            }
        }

        public void Connect(string serverIp, int serverPort,BroadCastClient broadCastClient)
        {
            if (!tcpClient.Connected)
            {
                tcpClient.Connect(serverIp, serverPort);
            }
            
            sendMessage(new ConnectMessage(broadCastClient));
            messageReader.ReadAsync(true);
        }
        private void sendMessage(NetworkMessage message)
        {
            NetworkMessageWriter networkWriter = new NetworkMessageWriter(tcpClient);
            networkWriter.WriteError += (_ob, _e) => { Logger.log.Error(_e); };
            networkWriter.WriteAsync(message);
        }
        public void SendVoiceMessage(VoiceMessage voiceMessage)
        {
            sendMessage(voiceMessage);
        }
    }
}
