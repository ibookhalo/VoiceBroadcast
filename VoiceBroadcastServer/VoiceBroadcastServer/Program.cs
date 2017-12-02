using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoiceBroadcastServer
{
    class Program
    {
        private static TCPServer tcpServer;
        static void Main(string[] args)
        {
            if (args.Length>0)
            {
                try
                {
                    var _args = args[0].Split(new char[] { ':' });
                    string ip = _args[0];
                    int port = int.Parse(_args[1]);
                    startServer(ip, port);
                }
                catch (Exception ex)
                {
                    stopServer();
                    Logger.log.Error(ex);
                }
            }
            else
            {
                Logger.log.Error("no args");
            }
        }
        private static void stopServer()
        {
            tcpServer?.Stop();
        }
        private static void startServer(string ip,int port)
        {
            try
            {
                tcpServer = new TCPServer();
                tcpServer.ClientConnected += TcpServer_ClientConnected;
                tcpServer.ClientDisconnected += TcpServer_ClientDisconnected;
                tcpServer.DataReceived += TcpServer_DataReceived;
                tcpServer.Start(ip, port);
                Logger.log.Info(string.Format("Server started on {0}:{1} ....", ip, port));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private static void TcpServer_DataReceived(ServerThread st, byte[] data)
        {
            
        }

        private static void TcpServer_ClientDisconnected(ServerThread st, string info)
        {
            
        }

        private static void TcpServer_ClientConnected(ServerThread st)
        {
            Logger.log.Info($"Client connected {st.Name}");
        }
    }
}
