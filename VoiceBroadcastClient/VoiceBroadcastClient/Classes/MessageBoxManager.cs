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
        public static void ShowMessageBoxErrorContactAdmin(string content)
        {
            showMessageBoxError(content, true);
        }
        private  static void showMessageBoxError(string content,bool adminFlag)
        {
            var m = content;
            if (adminFlag)
            {
                m += "\n\n\nBitte kontaktieren Sie Ihren Systemadministrator!";
            }
            MessageBox.Show(new Form() { TopMost = true }, m, AppDomain.CurrentDomain.FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        public static void ShowMessageBoxError(string content)
        {
            showMessageBoxError(content, false);
        }
    }
}
