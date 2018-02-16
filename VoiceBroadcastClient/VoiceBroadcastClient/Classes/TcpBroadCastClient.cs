using Network;
using Network.Messaging;
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace VoiceBroadcastClient.Classes
{
    class TcpBroadcastClient
    {
        private TcpClient tcpClient;
        private NetworkMessageReader messageReader;
        private NetworkMessageWriter messageWriter;

        public delegate void ClientConnected(object obj, ClientConnectedEventArgs e);
        public delegate void ClientVoiceMessageReceived(object obj, ClientVoiceMessageReceivedEventArgs e);
        public delegate void ClientDisonnected(object obj, EventArgs e);

        public event ClientConnected ClientConnectedEvent;
        private bool isClientDisconnectedEventAlreadyFired; /* will be set to 'false' every time the client (re)connect successfully
                                                               will be set to 'true' every time after firing ClientDisconnectedEvent event*/

        public event ClientDisonnected ClientDisconnectedEvent;
        public event ClientVoiceMessageReceived ClientVoiceMessageReceivedEvent;

        private object isConnectedLocker = new object();
        private volatile bool isConnected;
        public bool IsConnected
        {
            get
            {
                lock (isConnectedLocker)
                {
                    return isConnected;
                }
            }
            private set
            {
                lock (isConnectedLocker)
                {
                    this.isConnected = value;
                }
            }
        }

        private object isConnectingLocker = new object();
        private volatile bool isConnecting;
        private bool IsConnecting
        {
            get
            {
                lock (isConnectingLocker)
                {
                    return isConnected;
                }
            }
            set
            {
                lock (isConnectingLocker)
                {
                    this.isConnecting = value;
                }
            }
        }

        private System.Threading.Timer autoReconnectTimer;
        private const int autoReconnectTimerIntervalInMs = 8000;

        private IPAddress localIPAddress;

        public TcpBroadcastClient()
        {}
        private void autoReconnectTimerCallback(object state)
        {
            bool serverPingOK = isServerPingSuccess();

            if (!serverPingOK)
            {
                tcpClient.Close();
                handleClientDisconnected();
            }

            if (serverPingOK && !IsConnecting && !IsConnected)
            {
                // server is reachable
                // reconnect ...
                Logger.log.Warn("reconnecting ...");
                Connect();
            }
        }
        private bool isServerPingSuccess()
        {
            using (Ping serverPing = new Ping())
            {
                try
                {
                    // 4,5 sec ping timeout
                    PingReply replay = serverPing.Send(AppConfiguration.ReadConfig().ServerIP, 4500);
                    return replay.Status == IPStatus.Success;
                }
                catch (Exception ex)
                {
                    Logger.log.Warn(ex);
                    return false;
                }
            }
            return true;
        }
        private void handleClientDisconnected()
        {
            IsConnected = false;
            IsConnecting = false;

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
            try
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
            catch (Exception ex) 
            {
                Logger.log.Warn(ex);
            }
        }
        private void handleVoiceMessage(VoiceMessage voiceMessage, TcpClient tcpClient)
        {
            ClientVoiceMessageReceivedEvent?.BeginInvoke(this,new ClientVoiceMessageReceivedEventArgs(voiceMessage), null, null);
        }
        private void handleConnectMessage(ConnectMessage connectMessage, TcpClient tcpClient)
        {
            if (connectMessage.Connected 
                && connectMessage.BroadCastClient.Id!=null
                && connectMessage.BroadCastClient.Name!=null 
                && ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString().Equals(AppConfiguration.ReadConfig().ServerIP)
                && connectMessage.BroadCastClient.Name.Equals(AppConfiguration.ReadConfig().ClientName))
            {
                IsConnected = true;
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

            IsConnected = false;
            IsConnecting = true;

            var config = AppConfiguration.ReadConfig();
            try
            {
                tcpClient.BeginConnect(config.ServerIP, config.ServerPort, tcpClientConnectCallback, config);
            }
            catch (Exception ex)
            {
                IsConnected = false;
                IsConnecting = false;
                throw ex;
            }
            finally
            {
                if (autoReconnectTimer == null)
                {
                   autoReconnectTimer = new System.Threading.Timer(autoReconnectTimerCallback, null, autoReconnectTimerIntervalInMs, autoReconnectTimerIntervalInMs);
                }
            }

        }

        private void tcpClientConnectCallback(IAsyncResult ar)
        {
            try
            {
                if (tcpClient.Connected)
                {
                    var config = (AppConfiguration)ar.AsyncState;

                    localIPAddress = (tcpClient.Client.LocalEndPoint as IPEndPoint).Address;

                    sendMessage(new ConnectMessage(new BroadcastClient(config.ClientName, null)));
                    messageReader.ReadAsync(true);

                    IsConnecting = false;
                    IsConnected = true;
                }
                else
                {
                    IsConnecting = false;
                    IsConnected = false;
                }
            }
            catch (Exception ex)
            {
                tcpClient.Close();

                IsConnecting = false;
                IsConnected = false;

                Logger.log.Error(ex);
            }
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
