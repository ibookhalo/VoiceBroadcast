using NAudioWrapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
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
            try
            {
                var config = AppConfiguration.ReadConfig();

                cbOutput.Items.Clear();
                cbInput.Items.Clear();

                var audioDeviceEnum = new AudioDeviceEnemerator();
                var renderDevices = audioDeviceEnum.GetRenderDevices();
                var captureDevices = audioDeviceEnum.GetCaptureDevices();

                //Output
                cbOutput.Items.AddRange(renderDevices.ToArray());
                DeviceInfo itemToSelect = cbOutput.Items.Cast<DeviceInfo>().ToList().Find(di => di.ProductGuid.Equals(config.RenderDevice.ProductGuid));

                DeviceInfo noDeviceOutput = new DeviceInfo();
                cbOutput.Items.Add(noDeviceOutput); // kein Gerät
                
                if (itemToSelect!=null)
                {
                    // select
                    cbOutput.SelectedItem = itemToSelect;
                }
                else
                {
                    cbOutput.SelectedItem = noDeviceOutput;
                }
                config.RenderDevice = cbOutput.SelectedItem as DeviceInfo;

                //Input
                cbInput.Items.AddRange(captureDevices.ToArray());
                DeviceInfo itemToSelectInput = cbInput.Items.Cast<DeviceInfo>().ToList().Find(di => di.ProductGuid.Equals(config.CaptureDevice.ProductGuid));

                if (itemToSelectInput != null)
                {
                    // select
                    cbInput.SelectedItem = itemToSelectInput;
                }
                else if (captureDevices.Count>0)
                {
                    cbInput.SelectedIndex = 0;
                }
                else
                {
                    DeviceInfo noDeviceInput = new DeviceInfo();
                    cbInput.Items.Add(noDeviceInput);
                    cbInput.SelectedItem = noDeviceInput;
                }

                config.CaptureDevice = cbInput.SelectedItem as DeviceInfo;
                AppConfiguration.SaveConfig(config);
            }
            catch (Exception ex)
            {
                MessageBoxManager.ShowMessageBoxErrorContactAdmin(ex.StackTrace);
            }
        }
        private void ok_Click(object sender, EventArgs e)
        {
            IPAddress ipTemp;
            if (IPAddress.TryParse(tbServerIP.Text, out ipTemp))
            {
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    AppConfiguration.SaveConfig(
                        new AppConfiguration(tbServerIP.Text, (int)nudServerPort.Value, tbClientName.Text, cbInput.SelectedItem as DeviceInfo, cbOutput.SelectedItem as DeviceInfo));
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBoxManager.ShowMessageBoxErrorContactAdmin(ex.StackTrace);

                }
            }
            else
            {
                MessageBoxManager.ShowMessageBoxError("Bitte geben Sie eine gültige IP-Adresse ein.");
            }
        }

    }
}
