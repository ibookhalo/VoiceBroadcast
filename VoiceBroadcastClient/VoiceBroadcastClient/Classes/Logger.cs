using log4net.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoiceBroadcastClient
{
    public static class Logger
    {
        //private static string config = "<log4net>\r\n  <root>\r\n    <level value=\"ALL\" />\r\n    <appender-ref ref=\"console\" />\r\n    <appender-ref ref=\"file\" />\r\n  </root>\r\n  <appender name=\"console\" type=\"log4net.Appender.ConsoleAppender\">\r\n    <layout type=\"log4net.Layout.PatternLayout\">\r\n      <conversionPattern value=\"%date %level %logger - %message%newline\" />\r\n    </layout>\r\n  </appender>\r\n  <appender name=\"file\" type=\"log4net.Appender.RollingFileAppender\">\r\n    <file value=\"log.data\" />\r\n    <appendToFile value=\"true\" />\r\n    <rollingStyle value=\"Size\" />\r\n    <maxSizeRollBackups value=\"5\" />\r\n    <maximumFileSize value=\"20MB\" />\r\n    <staticLogFileName value=\"true\" />\r\n    <layout type=\"log4net.Layout.PatternLayout\">\r\n      <conversionPattern value=\"%date [%thread] %level %logger - %message%newline\" />\r\n    </layout>\r\n  </appender>\r\n</log4net>";
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static Stream generateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
        public static void EnableLog()
        {
            try
            {
                XmlConfigurator.Configure(generateStreamFromString(Properties.Resources.log4net));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
