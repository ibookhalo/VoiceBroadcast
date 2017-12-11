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
                if (config.RenderDevice.Id >= 0 && renderDevices.Exists(rd => rd.Id.Equals(config.RenderDevice.Id)))
                {
                    // device is active
                    cbOutput.Items.Add(config.RenderDevice);
                }
                renderDevices.RemoveAll(rd => rd.Id.Equals(config.RenderDevice.Id));
                cbOutput.Items.AddRange(renderDevices.ToArray());
                cbOutput.Items.Add(new DeviceInfo());

                cbOutput.SelectedIndex = 0;
                config.RenderDevice = cbOutput.SelectedItem as DeviceInfo;

                //Input

                if (config.CaptureDevice.Id >= 0 && captureDevices.Exists(cd => cd.Id.Equals(config.CaptureDevice.Id)))
                {
                    // device is active
                    cbInput.Items.Add(config.CaptureDevice);
                }
                captureDevices.RemoveAll(rd => rd.Id.Equals(config.CaptureDevice.Id));
                cbInput.Items.AddRange(captureDevices.ToArray());
                cbInput.Items.Add(new DeviceInfo());

                cbInput.SelectedIndex = 0;
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
                    string inputDeviceName = cbInput.SelectedItem.ToString();
                    string outputDeviceName = cbOutput.SelectedItem.ToString();

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
