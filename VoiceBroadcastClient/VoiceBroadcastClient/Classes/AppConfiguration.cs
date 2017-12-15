using NAudioWrapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace VoiceBroadcastClient
{
    [Serializable]
    public class AppConfiguration
    {
        public string ServerIP { get; set; }
        public int ServerPort { get; set; }
        public string ClientName { get; set; }
        public DeviceInfo CaptureDevice { get; set; }
        public DeviceInfo RenderDevice { get; set; }

        [NonSerialized()]
        private static AppConfiguration cachedConfigs;
        [NonSerialized()]
        private static readonly object appconfigLocker = new object();

        [NonSerialized()]
        private static readonly string CONFIG_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "voicebroadcast.conf");

        public AppConfiguration()
        {

        }
        public AppConfiguration(string ip,int port,string clientname,DeviceInfo captureDevice,DeviceInfo renderDevice)
        {
            this.ServerIP = ip;
            this.ServerPort = port;
            this.ClientName = clientname;
            this.CaptureDevice = captureDevice;
            this.RenderDevice = renderDevice;
        }
        public static AppConfiguration ReadConfig()
        {
            lock (appconfigLocker)
            {
                if (cachedConfigs == null)
                {
                    try
                    {
                        XmlSerializer ser = new XmlSerializer(typeof(AppConfiguration));
                        using (FileStream fs = new FileStream(CONFIG_PATH, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            return cachedConfigs = (AppConfiguration)ser.Deserialize(fs);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.log.Warn(ex);
                        try
                        {
                            // standard config ...
                            var standardConf = new AppConfiguration("127.0.0.1", 6666, "Clientname", new DeviceInfo(), new DeviceInfo());
                            serilize(standardConf);
                            return standardConf;
                        }
                        catch (Exception _ex)
                        {
                            Logger.log.Error(_ex);
                        }
                    }
                }

                return cachedConfigs;
            }
        }

        private static void serilize(AppConfiguration appConf)
        {
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(AppConfiguration));
                using (TextWriter WriteFileStream = new StreamWriter(CONFIG_PATH))
                {
                    ser.Serialize(WriteFileStream, appConf);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static void SaveConfig(AppConfiguration appConfiguration)
        {
            try
            {
                lock (appconfigLocker)
                {
                    serilize(appConfiguration);
                    cachedConfigs = appConfiguration;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
