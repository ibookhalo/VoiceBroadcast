using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Network;
using Network.Messaging;

namespace VoiceBroadcastServer
{
    class Server
    {
        private TcpListener tcpListener;
        private List<ServerBroadcastClient> clients;
        private uint lastClientID = 0;
        private object clientLocker = new object();
        public Server()
        {
            clients = new List<ServerBroadcastClient>();
        }

        public void Init(string ip,int port)
        {
            tcpListener = new TcpListener(new System.Net.IPEndPoint(IPAddress.Parse(ip), port));
        }

        public void AcceptClientsForEver()
        {
            tcpListener.Start();
            while (true)
            {
                try
                {
                    TcpClient newClient = tcpListener.AcceptTcpClient();
                    clients.Add(new ServerBroadcastClient(new BroadCastClient(null, null), newClient));

                    NetworkMessageReader messageReader = new NetworkMessageReader(newClient);
                    messageReader.ReadCompleted += MessageReader_ReadCompleted;
                    messageReader.ReadError += MessageReader_ReadError;
                    messageReader.ReadAsync(true);
                }
                catch (Exception ex)
                {
                    Logger.log.Error(ex);
                }
            }
        }

        private void MessageReader_ReadError(object obj, Network.EventArgs.NetworkMessageReaderReadErrorEventArgs e)
        {
            if (!e.TcpClient.Connected)
            {
                (obj as NetworkMessageReader).Dispose();
                lock (clients)
                {
                    clients.RemoveAll(client => client.TcpClient.Equals(e.TcpClient));
                }
            }
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
                messageWriter.WriteError += (_o, _e) => { Logger.log.Error(_e); };

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
                        messageReader.Dispose();
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
