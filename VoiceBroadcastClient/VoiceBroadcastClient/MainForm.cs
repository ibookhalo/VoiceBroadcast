using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinSound;

namespace VoiceBroadcastClient
{
    public partial class MainForm : Form
    {
        private NotifyIcon trayIcon = new NotifyIcon();
        private ConfigForm configForm;
        private TCPClient tcpClient;
        private bool isConnectedToServer;
        private bool allreadyShown,firstTimeShownTrayIcon;
        private System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

        public MainForm()
        {
            InitializeComponent();
            this.Disposed += FormBroadcastClient_Disposed;
            trayIcon.Icon = new Icon(Properties.Resources.appicon, 40, 40);
            trayIcon.Click += TrayIcon_Click;
            trayIcon.ContextMenu = new ContextMenu(new MenuItem[] {
                new MenuItem("Einstellungen",openConfig),
                new MenuItem("Schließen", exit)});
            setTrayIconStateToConnected(false);
            trayIcon.Visible = true;

            connectToServer();
        }

        private void TrayIcon_Click(object sender, EventArgs e)
        {
            if (isConnectedToServer)
            {
                show();
            }
        }
        private void setTrayIconStateToConnected(bool isConnected)
        {
            if (trayIcon != null)
            {
                trayIcon.Text = string.Format("{0} | {1}", AppDomain.CurrentDomain.FriendlyName, isConnected ? "Verbunden" : "Keine Verbindung");
            }
        }

        private void connectToServer()
        {
            try
            {
                var conf = AppConfiguration.ReadConfig();
                tcpClient = new TCPClient(conf.ServerIP, conf.ServerPort);
                tcpClient.AutoConnect = true;
                tcpClient.AutoConnectInterval = 5;
                tcpClient.ClientConnected += new TCPClient.DelegateConnection(OnClientConnected);
                tcpClient.ClientDisconnected += new TCPClient.DelegateConnection(OnClientDisconnected);
                tcpClient.ExceptionAppeared += new TCPClient.DelegateException(OnClientExceptionAppeared);
                tcpClient.DataReceived += new TCPClient.DelegateDataReceived(OnClientDataReceived);
                tcpClient.Connect();
                isConnectedToServer = true;
            }
            catch (Exception ex)
            {
                isConnectedToServer = false;
                Logger.log.Error(ex);
            }
        }

        private void OnClientDataReceived(TCPClient client, byte[] bytes)
        {
            
        }

        private void OnClientExceptionAppeared(TCPClient client, Exception ex)
        {
            isConnectedToServer = false;
            Logger.log.Error(ex);
            setTrayIconStateToConnected(false);
        }

        private void OnClientDisconnected(TCPClient client, string Info)
        {
            isConnectedToServer = false;
            Logger.log.Info("Client disconnected");
            setTrayIconStateToConnected(false);
        }

        private void OnClientConnected(TCPClient client, string Info)
        {
            isConnectedToServer = true;
            Logger.log.Info("Client connected");
            setTrayIconStateToConnected(true);
        }

        private void openConfig(object sender, EventArgs e)
        {
            if (configForm == null)
            {
                configForm = new ConfigForm();
            }
            if (!configForm.Visible)
            {
                configForm.ShowDialog();
            }
        }
        private void exit(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void show()
        {
            Left = Cursor.Position.X;
            Top = Screen.PrimaryScreen.WorkingArea.Bottom - this.Height - (firstTimeShownTrayIcon?-10:50);
            Visible = true;
            allreadyShown = true;
            Text = "Aufnehmen";
            Show();
            WindowState = FormWindowState.Normal;
            if (firstTimeShownTrayIcon==false)
            {
                firstTimeShownTrayIcon = true;
            }
            Activate();
        }
        private void hide()
        {
            Hide();
            Visible = false; // Hide form window.
            ShowInTaskbar = false; // Remove from taskbar.
        }
        private void FormBroadcastClient_Disposed(object sender, EventArgs e)
        {
            trayIcon.Visible = false;
            trayIcon.Dispose();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            hide();
        }

        private void MainForm_Deactivate(object sender, EventArgs e)
        {
            if (allreadyShown && Visible)
            {
                hide();
            }
        }

        private void btnRecord_MouseDown(object sender, MouseEventArgs e)
        {
            btnRecord.Image = Properties.Resources.Speak_On;
            sw.Start();
            timerDuration.Start();
            lblDuration.Visible = true;
            lblDuration.Text = "00:00:00";
        }

        private void btnRecord_MouseUp(object sender, MouseEventArgs e)
        {
            btnRecord.Image = Properties.Resources.Speak_Off;
            sw.Stop();
            sw.Reset();
            timerDuration.Stop();
            lblDuration.Visible = false;
        }

        private void timerDuration_Tick(object sender, EventArgs e)
        {
            lblDuration.Text = sw.Elapsed.ToString(@"hh\:mm\:ss");
        }

        private void btnRecord_Click(object sender, EventArgs e)
        {

        }
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason!=CloseReason.ApplicationExitCall)
            {
                e.Cancel = true;
                hide();
                Logger.log.Info("Closing app ...");
            }
        }
    }
}
