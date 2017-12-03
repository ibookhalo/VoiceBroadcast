using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Network;
using Network.Messaging;
using System.Threading;
using System.Runtime.InteropServices;
using System.Net.NetworkInformation;

namespace VoiceBroadcastServer
{
    class Server
    {
        private TcpListener tcpListener;
        private List<ServerBroadcastClient> clients;
        private uint lastClientID = 0;
        private object clientLocker = new object();
        private Timer nicStateCheckerTimer;
        private IPEndPoint localEndPoint;
        private DateTime lastTcpListenError;

        public Server()
        {
            clients = new List<ServerBroadcastClient>();
            nicStateCheckerTimer = new Timer(adapterStateCheckerTimerCallback, null, 5, 5000); // every 5 sec check nic state ...
        }

        private void adapterStateCheckerTimerCallback(object state)
        {
            Logger.log.Info("Timer ...");
            lock (tcpListener)
            {
                var nic = getNetworkAdapterByIP(localEndPoint.Address);
                if (nic != null)
                {
                    if (nic.OperationalStatus != OperationalStatus.Up)
                    {
                        // cable unplugged ?
                        tcpListener.Stop();
                    }
                }
                else
                {
                    // nic is disable ?
                    tcpListener.Stop();
                }

            }
        }
        private bool Connected
        {
            get
            {
                try
                {
                    if (tcpListener.Server != null)
                    {
                        // Detect if client disconnected
                        if (tcpListener.Server.Poll(1, SelectMode.SelectRead) && tcpListener.Server.Available == 0)
                        {
                            byte[] buff = new byte[1];
                            if (tcpListener.Server.Receive(buff, SocketFlags.Peek) == 0)
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
        private NetworkInterface getNetworkAdapterByIP(IPAddress ip)
        {
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    foreach (UnicastIPAddressInformation ipInfo in nic.GetIPProperties().UnicastAddresses)
                    {
                        if (ipInfo.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && ipInfo.Address.Equals(ip))
                        {
                            return nic;
                        }
                    }
                }
            }

            return null;
        }
        public void Init(string ip, int port)
        {
            tcpListener = new TcpListener(localEndPoint = new System.Net.IPEndPoint(IPAddress.Parse(ip), port));
        }

        private void restartTcpListener()
        {
            lock (tcpListener)
            {
                try
                {
                    tcpListener.Stop();
                    tcpListener.Start();
                }
                catch
                {}
            }
        }
        public void AcceptClientsForEver()
        {
            lock (tcpListener)
            {
                tcpListener.Start();
            }

            while (true)
            {
                try
                {
                    Logger.log.Info("Waiting for clients ...");
                    TcpClient newClient = tcpListener.AcceptTcpClient();

                    clients.Add(new ServerBroadcastClient(new BroadCastClient(null, null), newClient));

                    NetworkMessageReader messageReader = new NetworkMessageReader(newClient);
                    messageReader.ReadCompleted += MessageReader_ReadCompleted;
                    messageReader.ReadError += MessageReader_ReadError;

                    messageReader.StopReadingOnError = true;
                    messageReader.ReadAsync(true);
                }
                catch (Exception ex)
                {
                    Logger.log.Error(ex);

                    if ((DateTime.Now-lastTcpListenError).TotalMilliseconds < 1000) 
                    {
                        // every 10 sec
                        Thread.Sleep(10000);
                    }
                    restartTcpListener();
                    lastTcpListenError = DateTime.Now;
                }
            }
        }
        

        private void removeClientFromListByTcpClient(TcpClient tcpClient)
        {
            lock (clients)
            {
                clients.RemoveAll(client => client.TcpClient.Equals(tcpClient));
            }
        }
        private void MessageReader_ReadError(object obj, Network.EventArgs.NetworkMessageReaderReadErrorEventArgs e)
        {
            removeClientFromListByTcpClient(e.TcpClient);
        }
        private void MessageWriter_WriteError(object obj, Network.EventArgs.NetworkMessageWriterWriteErrorEventArgs e)
        {
            removeClientFromListByTcpClient(e.TcpClient);
        }
        private void MessageReader_ReadCompleted(object obj, Network.EventArgs.NetworkMessageReaderReadCompletedEventArgs e)
        {
            if (e.NetworkMessage is ConnectMessage)
            {
                handleConnectMessage(e.NetworkMessage as ConnectMessage, e.TcpClient, obj as NetworkMessageReader);
            }
            else if (e.NetworkMessage is VoiceMessage)
            {
                handleVoiceMessage(e.NetworkMessage as VoiceMessage, e.TcpClient, obj as NetworkMessageReader);
            }
        }
        private bool existsClientIdInClientList(uint clientID)
        {
            return clients.Exists(client => client.Client.Id.Value.Equals(clientID));
        }
        private void handleVoiceMessage(VoiceMessage voiceMessage, TcpClient sender, NetworkMessageReader networkMessageReader)
        {
            if (voiceMessage.Sender.Id.HasValue && existsClientIdInClientList(voiceMessage.Sender.Id.Value))
            {
                foreach (ServerBroadcastClient client in clients.Where(client => !client.Client.Id.Value.Equals(voiceMessage.Sender.Id.Value)).ToList())
                {
                    // sendBroadcast
                    try
                    {
                        NetworkMessageWriter messageWriter = new NetworkMessageWriter(client.TcpClient);
                        messageWriter.WriteError += (_ob, _e) => { Logger.log.Error(_e); };
                        messageWriter.WriteAsync(voiceMessage);
                    }
                    catch (Exception ex)
                    {
                        Logger.log.Error(ex);
                    }
                }
            }
        }
        private void handleConnectMessage(ConnectMessage connectMessage, TcpClient sender, NetworkMessageReader messageReader)
        {
            try
            {
                NetworkMessageWriter messageWriter = new NetworkMessageWriter(sender);
                messageWriter.StopWritingOnError = true;
                messageWriter.WriteError += MessageWriter_WriteError;

                if (connectMessage.BroadCastClient.Id.HasValue)
                {
                    // client has id ...
                    // check if client id is in the list
                    var clientInList = clients.Find(client => client.Client.Id.Value.Equals(connectMessage.BroadCastClient.Id.Value));
                    if (clientInList != null)
                    {
                        lock (clients)
                        {
                            // refresh client in the list
                            clients.Remove(clientInList);
                            clients.Add(new ServerBroadcastClient(new BroadCastClient(connectMessage.BroadCastClient.Name,
                                                                    connectMessage.BroadCastClient.Id), sender));
                        }
                        connectMessage.Connected = true;
                        messageWriter.WriteAsync(connectMessage);

                        Logger.log.Info($"client connected {connectMessage.BroadCastClient}");
                    }
                    else
                    {
                        // disconnect client ...
                        //
                    }
                }
                else
                {
                    // client want to connect and does not have an id ...
                    var newClient = new BroadCastClient(connectMessage.BroadCastClient.Name, ++lastClientID);
                    lock (clients)
                    {
                        clients.Add(new ServerBroadcastClient(newClient,sender));
                    }
                    // send client ConnectAck Message
                    connectMessage.Connected = true;
                    connectMessage.BroadCastClient = newClient;
                    messageWriter.WriteAsync(connectMessage);

                    Logger.log.Info($"New client connected {connectMessage.BroadCastClient}");
                }
            }
            catch (Exception ex)
            {
                Logger.log.Error(ex);
            }
        }

    }
 }
