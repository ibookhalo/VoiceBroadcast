using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VoiceBroadcastClient
{
    public partial class ConfigForm : Form
    {
        public ConfigForm()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            var config = AppConfiguration.ReadConfig();
            tbClientName.Text = config.ClientName;
            tbServerIP.Text = config.ServerIP;
            nudServerPort.Value = config.ServerPort;
            InitComboboxes();
        }

        private void InitComboboxes()
        {
            var config=AppConfiguration.ReadConfig();

            cbOutput.Items.Clear();
            cbInput.Items.Clear();

            List<String> playbackNames = WinSound.WinSound.GetPlaybackNames();
            List<String> recordingNames = WinSound.WinSound.GetRecordingNames();

            //Output
            cbOutput.Items.Add(config.OutputDeviceName);
            foreach (String name in playbackNames.Where(x => x != null))
            {
                cbOutput.Items.Add(name);
            }

            cbInput.Items.Add(config.InputDeviceName);
            //Input
            foreach (String name in recordingNames.Where(x => x != null))
            {
                cbInput.Items.Add(name);
            }

            //Output
            if (cbOutput.Items.Count > 0)
            {
                cbOutput.SelectedIndex = 0;
            }
            //Input
            if (cbInput.Items.Count > 0)
            {
                cbInput.SelectedIndex = 0;
            }
        }
        private void ok_Click(object sender, EventArgs e)
        {
            try
            {
                AppConfiguration.SaveConfig(new AppConfiguration(tbServerIP.Text, (int)nudServerPort.Value, tbClientName.Text,
                    cbInput.SelectedIndex>=0 ? cbInput.SelectedItem.ToString():"Kein",
                    cbOutput.SelectedIndex >= 0 ? cbOutput.SelectedItem.ToString() : "Kein"));
                this.Close();
            }
            catch (Exception ex)
            {
                Logger.log.Error(ex);
            }
        }
    }
}
