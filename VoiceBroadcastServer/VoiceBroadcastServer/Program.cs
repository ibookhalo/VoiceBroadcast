using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoiceBroadcastServer
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length>=1)
            {
                try
                {
                    Server server = new Server();

                    var _args = args[0].Split(new char[] { ':' });
                    server.Init(_args[0], int.Parse(_args[1]));

                    Logger.log.Info("Server started ...");
                    Logger.log.Info("Waiting for clients ...");
                    server.AcceptClientsForEver();
                }
                catch (Exception ex)
                {
                    Logger.log.Error(ex);
                }
            }
            else
            {
                Logger.log.Error("Args error !");
            }
        }
    }
}
