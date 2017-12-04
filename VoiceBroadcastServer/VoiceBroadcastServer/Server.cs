using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Network;
using Network.Messaging;
using System.Threading;
using System.Net.NetworkInformation;

namespace VoiceBroadcastServer
{
    class Server
    {
        private TcpListener tcpListener;
        private List<ServerBroadcastClient> clients;
        private uint lastClientID = 0;
        private NetworkInterfaceStateNotifier nicNotifier;
        private IPEndPoint localEndPoint;
        public Server()
        {
            clients = new List<ServerBroadcastClient>();
            
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
        public void Init(string ip, int port)
        {
            tcpListener = new TcpListener(localEndPoint = new System.Net.IPEndPoint(IPAddress.Parse(ip), port));
        }
        private bool restartTcpListener()
        {
            lock (tcpListener)
            {
                try
                {
                    tcpListener.Stop();
                    tcpListener.Start();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
        public void AcceptClientsForEver()
        {
            lock (tcpListener)
            {
                tcpListener.Start();
            }
            if (nicNotifier==null)
            {
                nicNotifier = new NetworkInterfaceStateNotifier(5000, localEndPoint.Address);
                nicNotifier.NetworkInterfaceIsNotUpEvent += NicNotifier_NetworkInterfaceIsNotUpEvent;
            }

            while (true)
            {
                try
                {
                    Logger.log.Info("Waiting for clients ...");

                    NetworkMessageReader messageReader = new NetworkMessageReader(tcpListener.AcceptTcpClient());
                    messageReader.ReadCompleted += MessageReader_ReadCompleted;
                    messageReader.ReadError += MessageReader_ReadError;

                    messageReader.OnErrorStopReadingAndCloseClient = true;
                    messageReader.ReadAsync(true);
                }
                catch (Exception ex)
                {
                    Logger.log.Error(ex);
                    bool needTcpListenerRestart = true;
                    while (needTcpListenerRestart)
                    {
                        while (restartTcpListener())
                        {
                            needTcpListenerRestart = false;
                            break;
                        }
                        Thread.Sleep(5000); // wait 5 sec
                    }
                }
            }
        }

        private void NicNotifier_NetworkInterfaceIsNotUpEvent(object obj, EventArgs e)
        {
            lock (tcpListener)
            {
                tcpListener.Stop(); // interrupt  tcpListener.AcceptTcpClient() ...
            }
        }

        private void removeClientFromListByTcpClient(TcpClient tcpClient,bool closeClient=false)
        {
            if (closeClient)
            {
                Logger.log.Info($"Closing client connection: {getServerBroadcastClientByTcpClient(tcpClient)?.Client}");
                tcpClient?.Close();
            }
            lock (clients)
            {
                clients.RemoveAll(client => client.TcpClient.Equals(tcpClient));
            }
        }
        private void removeClientsFromListByTcpClients(List<TcpClient> tcpClients,bool closeClients=false)
        {
            tcpClients.ForEach(client => removeClientFromListByTcpClient(client,closeClients));
        }
        private void MessageReader_ReadError(object obj, Network.EventArgs.NetworkMessageErrorEventArgs e)
        {
            Logger.log.Info($"Read error: {getServerBroadcastClientByTcpClient(e.TcpClient)?.Client}");
            removeClientFromListByTcpClient(e.TcpClient,true);
        }
        private void MessageWriter_WriteError(object obj, Network.EventArgs.NetworkMessageWriterWriteErrorEventArgs e)
        {
            removeClientFromListByTcpClient(e.TcpClient,true);
        }
        private void MessageReader_ReadCompleted(object obj, Network.EventArgs.NetworkMessageReaderReadCompletedEventArgs e)
        {
            try
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
            catch (Exception ex) 
            {
                Logger.log.Error(ex);
            }
        }
        private bool existsClientInClientListByTcpClientAndId(TcpClient tcpClient,uint clientID)
        {
            return clients.Exists(client => client.Client.Id.Value.Equals(clientID) && client.TcpClient.Equals(tcpClient));
        }

        private ServerBroadcastClient getServerBroadcastClientByTcpClient(TcpClient tcpClient)
        {
            return clients.Find(client => client.TcpClient.Equals(tcpClient));
        }
        private void handleVoiceMessage(VoiceMessage voiceMessage, TcpClient sender, NetworkMessageReader networkMessageReader)
        {
            if (voiceMessage.Sender.Id.HasValue && existsClientInClientListByTcpClientAndId(sender,voiceMessage.Sender.Id.Value))
            {
                Logger.log.Info($"Voicemessage received from {getServerBroadcastClientByTcpClient(sender)}");

                var clientsToRemove = new List<TcpClient>();

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
                        // connection error? client is offline? 
                        Logger.log.Error(ex);
                        clientsToRemove.Add(client.TcpClient); 
                    }
                }

                removeClientsFromListByTcpClients(clientsToRemove);
            }
        }
        private void handleConnectMessage(ConnectMessage connectMessage, TcpClient sender, NetworkMessageReader messageReader)
        {
            try
            {
                if (connectMessage.BroadCastClient==null)
                {
                    // close
                    sender.Close();
                    return;
                }

                if (connectMessage.BroadCastClient.Name!=null && connectMessage.BroadCastClient.Name.Length>2)
                {
                    var newClient = new BroadcastClient(connectMessage.BroadCastClient.Name, ++lastClientID);
                    lock (clients)
                    {
                        clients.Add(new ServerBroadcastClient(newClient, sender));
                    }

                    NetworkMessageWriter messageWriter = new NetworkMessageWriter(sender);
                    messageWriter.OnErrorStopWritingAndCloseClient = true;
                    messageWriter.WriteError += MessageWriter_WriteError;

                    // send client ConnectMessage Message
                    connectMessage.Connected = true;
                    connectMessage.BroadCastClient = newClient; // modified ..
                    messageWriter.WriteAsync(connectMessage);

                    Logger.log.Info($"client connected {connectMessage.BroadCastClient}");
                }
                else
                {
                    // close
                    sender.Close();
                }
            }
            catch (Exception ex)
            {
                Logger.log.Error(ex);
            }
        }

    }
 }
