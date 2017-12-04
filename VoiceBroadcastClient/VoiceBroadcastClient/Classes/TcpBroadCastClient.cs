using Network;
using Network.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace VoiceBroadcastClient.Classes
{
    class TcpBroadcastClient
    {
        private TcpClient tcpClient;
        private NetworkMessageReader messageReader;
        private NetworkMessageWriter messageWriter;

        public delegate void ClientConnected(object obj, ClientConnectedEventArgs e);
        public delegate void ClientDisonnected(object obj, EventArgs e);

        public event ClientConnected ClientConnectedEvent;
        public event ClientDisonnected ClientDisconnectedEvent;

        public bool IsConnected { get; private set; }
        private object isConnectedLocker = new object();
       
        private System.Threading.Timer autoReconnectTimer;
        private const int autoReconnectTimerInterval = 5000;
        
        public TcpBroadcastClient()
        {
        }

        private void autoReconnectTimerCallback(object state)
        {
            lock (isConnectedLocker)
            {// todo
                if (!IsConnected)
                {
                    try
                    {
                        Logger.log.Warn("Reconnecting ...");
                        // reconnect
                        tcpClient?.Close();
                        Connect();
                    }
                    catch (Exception ex)
                    {
                        Logger.log.Error(ex);
                    }
                }
            }
        }

        private void isConnected(bool isConnected)
        {
            lock (isConnectedLocker)
            {
                IsConnected = isConnected;
            }
        }
        private void handleClientDisconnected()
        {
            isConnected(false);
            ClientDisconnectedEvent?.BeginInvoke(this, new EventArgs(), null, null);
        }
        private void MessageReader_ReadError(object obj, Network.EventArgs.NetworkMessageErrorEventArgs e)
        {
            handleClientDisconnected();
        }
        private void MessageWriter_WriteError(object obj, Network.EventArgs.NetworkMessageWriterWriteErrorEventArgs e)
        {
            handleClientDisconnected();
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
            if (connectMessage.Connected 
                && connectMessage.BroadCastClient.Id!=null
                && connectMessage.BroadCastClient.Name!=null 
                && ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString().Equals(AppConfiguration.ReadConfig().ServerIP)
                && connectMessage.BroadCastClient.Name.Equals(AppConfiguration.ReadConfig().ClientName))
            {
                isConnected(true);
                ClientConnectedEvent?.BeginInvoke(this,new ClientConnectedEventArgs(connectMessage.BroadCastClient), null, null);
            }
            else
            {
                // close
                tcpClient.Close();
            }
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

        // TODO Threadsicher???!!!!
        public void Connect()
        {
            if (autoReconnectTimer == null)
            {
                autoReconnectTimer = new System.Threading.Timer(autoReconnectTimerCallback, null, autoReconnectTimerInterval+5000, autoReconnectTimerInterval);
            }

            tcpClient = new TcpClient();
            messageReader = new NetworkMessageReader(tcpClient);
            messageWriter = new NetworkMessageWriter(tcpClient);

            messageReader.OnErrorStopReadingAndCloseClient = true;
            messageWriter.OnErrorStopWritingAndCloseClient = true;

            messageReader.ReadCompleted += MessageReader_ReadCompleted;
            messageReader.ReadError += MessageReader_ReadError;
            messageWriter.WriteError += MessageWriter_WriteError;

            var config = AppConfiguration.ReadConfig();

            lock (isConnectedLocker)
            {
                tcpClient.Connect(config.ServerIP, config.ServerPort);
            }

            sendMessage(new ConnectMessage(new BroadcastClient(config.ClientName,null)));
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
