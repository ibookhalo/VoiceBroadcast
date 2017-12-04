using Network;
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
using VoiceBroadcastClient.Classes;
using WinSound;

namespace VoiceBroadcastClient
{
    public partial class MainForm : Form
    {
        private NotifyIcon appTaskbarIcon;
        private ConfigForm configForm;
        private bool allreadyShown,firstTimeShownTrayIcon;
        private NotifyIcon notifyIconBalloon;
        private TcpBroadcastClient client;

        private BroadcastClient broadcastClient;

        private System.Diagnostics.Stopwatch stopWatch;
        public MainForm()
        {
            InitializeComponent();

            client = new TcpBroadcastClient();
            appTaskbarIcon = new NotifyIcon();
            stopWatch = new System.Diagnostics.Stopwatch();

            appTaskbarIcon.MouseClick += TrayIcon_MouseClick;
            Disposed += FormBroadcastClient_Disposed;

            appTaskbarIcon.Icon = new Icon(Properties.Resources.appIcon, 40, 40);
            appTaskbarIcon.ContextMenu = new ContextMenu(new MenuItem[] {
                new MenuItem("Einstellungen",openConfig),
                new MenuItem("Beenden", exit)});
            appTaskbarIcon.Visible = true;

            setFormAboveWindowsTaskBar();

            refreshAppTaskbarIconState(false);
            connectToServer();
        }

        private void TrayIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button==MouseButtons.Left)
            {
                if (!client.IsConnected)
                {
                    showNotificationBalloon("Voicebroadcast ist nicht mit dem Server verbunden !");
                    return;
                }

                refreshAppTaskbarIconState(true);
                hideForm();
                showForm();
            }
        }

        private void showNotificationBalloon(string message)
        {
            if (notifyIconBalloon==null)
            {
                notifyIconBalloon = new NotifyIcon();

                notifyIconBalloon.BalloonTipClosed += NotifyIconBalloon_DisposeBalloonTip;
                notifyIconBalloon.BalloonTipClicked += NotifyIconBalloon_DisposeBalloonTip;

                notifyIconBalloon.Icon = Properties.Resources.appIcon;
                notifyIconBalloon.Visible = true;
                notifyIconBalloon.ShowBalloonTip(2000, Text, message,ToolTipIcon.Error);
            }
            else
            {
                notifyIconBalloon.Dispose();
                notifyIconBalloon = null;
                showNotificationBalloon(message);
            }
        }

        private void NotifyIconBalloon_DisposeBalloonTip(object sender, EventArgs e)
        {
            notifyIconBalloon.Visible = false;
            notifyIconBalloon.Dispose();
        }

        private void refreshAppTaskbarIconState(bool connected)
        {
            appTaskbarIcon.Icon = connected ? Properties.Resources.appIconOn : Properties.Resources.appIconOff;
            appTaskbarIcon.Text = string.Format("{0} | {1}", AppDomain.CurrentDomain.FriendlyName, connected ? "Verbunden" : "Keine Verbindung");
        }
        private void connectToServer()
        {
            try
            {
                var conf = AppConfiguration.ReadConfig();
                client.ClientConnectedEvent += Client_ClientConnectedEvent;
                client.ClientDisconnectedEvent += Client_ClientDisconnectedEvent;

                client.Connect();
                //
            }
            catch (Exception ex)
            {
                Logger.log.Error(ex);
            }
        }

        private void Client_ClientDisconnectedEvent(object obj, EventArgs e)
        {
            refreshAppTaskbarIconState(false);
        }

        private void Client_ClientConnectedEvent(object obj, ClientConnectedEventArgs e)
        {
            broadcastClient = e.BroadcastClient;

            refreshAppTaskbarIconState(true);
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
                configForm.Close();
                configForm = null;
            }
        }
        private void exit(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void setFormAboveWindowsTaskBar()
        {
            Left = Cursor.Position.X;
            Top = Screen.PrimaryScreen.WorkingArea.Bottom - this.Height - (firstTimeShownTrayIcon ? -10 : 50);
        }
        private void showForm()
        {
            setFormAboveWindowsTaskBar();
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
        private void hideForm()
        {
            Hide();
            Visible = false; // Hide form window.
            ShowInTaskbar = false; // Remove from taskbar.
        }
        private void FormBroadcastClient_Disposed(object sender, EventArgs e)
        {
            appTaskbarIcon.Visible = false;
            appTaskbarIcon.Dispose();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            hideForm();
        }
        private void MainForm_Deactivate(object sender, EventArgs e)
        {
            if (allreadyShown && Visible)
            {
                hideForm();
            }
        }
        private void btnRecord_MouseDown(object sender, MouseEventArgs e)
        {

            btnRecord.Image = Properties.Resources.Speak_On;
            stopWatch.Start();
            timerDuration.Start();
            lblDuration.Visible = true;
            lblDuration.Text = "00:00:00";
            
            // start recording

            
        }

        private void btnRecord_MouseUp(object sender, MouseEventArgs e)
        {
            btnRecord.Image = Properties.Resources.Speak_Off;
            stopWatch.Stop();
            stopWatch.Reset();
            timerDuration.Stop();
            lblDuration.Visible = false;
            // stop recording

            // send voiceMessage ...
            client.SendVoiceMessage(new Network.Messaging.VoiceMessage(broadcastClient, null));

        }

        private void timerDuration_Tick(object sender, EventArgs e)
        {
            lblDuration.Text = stopWatch.Elapsed.ToString(@"hh\:mm\:ss");
        }
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason!=CloseReason.ApplicationExitCall)
            {
                e.Cancel = true;
                hideForm();
            }
        }
    }
}
