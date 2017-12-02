using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoiceBroadcastClient
{
    class AppConfiguration
    {
        public AppConfiguration(string ip,int port,string clientname,string inputDeviceName,string outputDeviceName)
        {
            this.ServerIP = ip;
            this.ServerPort = port;
            this.ClientName = clientname;
            this.InputDeviceName = inputDeviceName;
            this.OutputDeviceName = outputDeviceName;
        }
        public static AppConfiguration ReadConfig()
        {
            return  new AppConfiguration(
                Properties.Settings.Default.serverIP,
                Properties.Settings.Default.serverPort,
               Properties.Settings.Default.clientName,
               Properties.Settings.Default.inputDeviceName,
               Properties.Settings.Default.outputDeviceName);
        }

        public static void SaveConfig(AppConfiguration appConfiguration)
        {
            Properties.Settings.Default.serverIP= appConfiguration.ServerIP;
            Properties.Settings.Default.serverPort= appConfiguration.ServerPort;
            Properties.Settings.Default.clientName= appConfiguration.ClientName;
            Properties.Settings.Default.inputDeviceName = appConfiguration.InputDeviceName;
            Properties.Settings.Default.outputDeviceName = appConfiguration.OutputDeviceName;
            Properties.Settings.Default.Save();
        }

        public string ServerIP { get; }
        public int ServerPort { get; }
        public string ClientName { get; }
        public string InputDeviceName { get; }
        public string OutputDeviceName { get; }
    }
}
