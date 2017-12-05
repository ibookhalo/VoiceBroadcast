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

        private bool isClientDisconnectedEventAlreadyFired; /* will be set to 'false' every time the client (re)connect successfully
                                                               will be set to 'true' every time after firing ClientDisconnectedEvent event*/

        public event ClientDisonnected ClientDisconnectedEvent;

        public bool IsConnected { get; private set; }
        private object isConnectedLocker = new object();
        
        private bool isConnecting;
        private object isConnectingLocker = new object();

        private System.Threading.Timer autoReconnectTimer;
        private const int autoReconnectTimerIntervalInMs = 5000;

        private IPAddress localIPAddress;

        public TcpBroadcastClient()
        {
        }

        private void autoReconnectTimerCallback(object state)
        {
            bool isConnected = false;
            bool isConnecting = false;

            lock (isConnectedLocker)
            {
                isConnected = this.IsConnected;    
            }
            lock (isConnectingLocker)
            {
                isConnecting = this.isConnecting;
            }

            bool isNicUp= NetworkInfoRetriever.IsNetworkAdapterUp(localIPAddress);
            if (!isNicUp)
            {
                tcpClient.Close();
                handleClientDisconnected();
            }

            if (isNicUp && !isConnecting && !isConnected)
            {
                // reconnect ...
                Logger.log.Warn("reconnecting ...");
                Connect();
            }
        }

        private void setIsConnected(bool isConnected)
        {
            lock (isConnectedLocker)
            {
                IsConnected = isConnected;
            }
        }
        private void setIsConnecting(bool isConnecting)
        {
            lock (isConnectingLocker)
            {
                this.isConnecting = isConnecting;
            }
        }
        private void handleClientDisconnected()
        {
            setIsConnected(false);
            if (!isClientDisconnectedEventAlreadyFired)
            {
                ClientDisconnectedEvent?.BeginInvoke(this, new EventArgs(), null, null);
                isClientDisconnectedEventAlreadyFired = true;
            }
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
                setIsConnected(true);
                isClientDisconnectedEventAlreadyFired = false;
                ClientConnectedEvent?.BeginInvoke(this,new ClientConnectedEventArgs(connectMessage.BroadCastClient), null, null);
            }
            else
            {
                // close
                tcpClient.Close();
            }
        }
        
        // TODO Threadsicher???!!!!
        public void Connect()
        {
            tcpClient = new TcpClient();
            messageReader = new NetworkMessageReader(tcpClient);
            messageWriter = new NetworkMessageWriter(tcpClient);

            messageReader.OnErrorStopReadingAndCloseClient = true;
            messageWriter.OnErrorStopWritingAndCloseClient = true;

            messageReader.ReadCompleted += MessageReader_ReadCompleted;
            messageReader.ReadError += MessageReader_ReadError;
            messageWriter.WriteError += MessageWriter_WriteError;

            setIsConnected(false);
            setIsConnecting(true);

            try
            {
                var config = AppConfiguration.ReadConfig();

                tcpClient.Connect(config.ServerIP, config.ServerPort);

                if (autoReconnectTimer == null)
                {
                    autoReconnectTimer = new System.Threading.Timer(autoReconnectTimerCallback, null, autoReconnectTimerIntervalInMs, autoReconnectTimerIntervalInMs);
                }
                localIPAddress = (tcpClient.Client.LocalEndPoint as IPEndPoint).Address;

                sendMessage(new ConnectMessage(new BroadcastClient(config.ClientName, null)));
                messageReader.ReadAsync(true);
            }
            catch (Exception ex)
            {
                tcpClient.Close();

                setIsConnecting(false);
                setIsConnected(false);

                throw ex;
            }

            setIsConnecting(false);
            setIsConnected(true);

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
