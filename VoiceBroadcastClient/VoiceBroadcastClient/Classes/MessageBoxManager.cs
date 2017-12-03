using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VoiceBroadcastClient
{
    public static class MessageBoxManager
    {
        public static void ShowMessageBoxError(string content)
        {
            MessageBox.Show($"{content}\n\n\nBitte kontaktieren Sie Ihren Systemadministrator!", AppDomain.CurrentDomain.FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
