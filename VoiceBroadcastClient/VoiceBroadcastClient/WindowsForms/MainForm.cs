using Network;
using Network.Messaging;
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

namespace VoiceBroadcastClient
{
    public partial class MainForm : Form
    {
        private NotifyIcon appTaskbarIcon, voiceMessageReceivedBallonTip;
        private ConfigForm configForm;
        private bool allreadyShown, firstTimeShownTrayIcon;
        private TcpBroadcastClient client;

        private BroadcastClient broadcastClient;
        private int timerRecordingSec;

        private NAudioWrapper.Recorder soundRecorder;
        private NAudioWrapper.Player soundPlayer;

        public MainForm()
        {
            InitializeComponent();

            client = new TcpBroadcastClient();
            client.ClientVoiceMessageReceivedEvent += Client_ClientVoiceMessageReceivedEvent;
            client.ClientConnectedEvent += Client_ClientConnectedEvent;
            client.ClientDisconnectedEvent += Client_ClientDisconnectedEvent;

            appTaskbarIcon = new NotifyIcon();

            appTaskbarIcon.MouseClick += TrayIcon_MouseClick;
            Disposed += FormBroadcastClient_Disposed;

            appTaskbarIcon.Icon = new Icon(Properties.Resources.appIcon, 40, 40);
            appTaskbarIcon.ContextMenu = new ContextMenu(new MenuItem[] {
                new MenuItem("Einstellungen",openConfig),
                new MenuItem("Beenden", exit)});
            appTaskbarIcon.Visible = true;

            setFormAboveWindowsTaskBar();

            setAppTaskbarIconState(false);
            connectToServer();
        }
        private void showVoiceMessageReceivedBallonTip(VoiceMessage voiceMessage)
        {
            disposeVoiceMessageReceivedBallonTip();

            voiceMessageReceivedBallonTip = new NotifyIcon();
            voiceMessageReceivedBallonTip.BalloonTipClosed += VoiceMessageReceivedBallonTip_BalloonTipClosed;
            voiceMessageReceivedBallonTip.BalloonTipClicked += VoiceMessageReceivedBallonTip_BalloonTipClosed;
            voiceMessageReceivedBallonTip.Icon = Properties.Resources.appIcon;
            voiceMessageReceivedBallonTip.Visible = true;

            var message = $"Von {voiceMessage.Sender.Name} empfangen";
            voiceMessageReceivedBallonTip.ShowBalloonTip(2000, "Broadcast", message, ToolTipIcon.Info);
        }
        private void disposeVoiceMessageReceivedBallonTip()
        {
            if (voiceMessageReceivedBallonTip != null)
            {
                voiceMessageReceivedBallonTip.Visible = false;
                voiceMessageReceivedBallonTip.Dispose();
            }
        }
        private void VoiceMessageReceivedBallonTip_BalloonTipClosed(object sender, EventArgs e)
        {
            disposeVoiceMessageReceivedBallonTip();
        }
        private void Client_ClientVoiceMessageReceivedEvent(object obj, ClientVoiceMessageReceivedEventArgs e)
        {
            executeCodeOnUIThread(() =>
            {
                showVoiceMessageReceivedBallonTip(e.VoiceMessage);
                var conf = AppConfiguration.ReadConfig();

                if (conf.RenderDevice.Id >= 0)
                {
                    if (soundPlayer != null)
                    {
                        // todo warte schlange !!
                    }
                    soundPlayer = new NAudioWrapper.Player(AppConfiguration.ReadConfig().RenderDevice.Id);
                    soundPlayer.PlaybackStopped += soundPlayer_PlaybackStopped;
                    soundPlayer.Play(e.VoiceMessage.Data);
                }
                else
                {
                    executeCodeOnUIThread(() =>
                    {
                        MessageBoxManager.ShowMessageBoxError("Fehler beim Abspielen der Broadcast-Nachricht, da kein Ausgabegerät gefunden werden konnte.");
                    });
                }
            });
        }
        private void soundPlayer_PlaybackStopped(object sender, EventArgs e)
        {
            soundPlayer.Stop();
        }
        private void TrayIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (!client.IsConnected)
                {
                    MessageBoxManager.ShowMessageBoxError("Voicebroadcast hat keine Verbindung zum Server!");
                }
                else if (AppConfiguration.ReadConfig().CaptureDevice.Id <=-1)
                {
                    MessageBoxManager.ShowMessageBoxError("Voicebroadcast konnte kein Aufnamegerät (Mikrofon) finden!.\nBitte überprüfen Sie die Einstellungen.");
                }
                else
                {
                    setAppTaskbarIconState(true);
                    hideForm();
                    showForm();
                }
            }
        }
        private void setAppTaskbarIconState(bool connected)
        {
            appTaskbarIcon.Icon = connected ? Properties.Resources.appIconOn : Properties.Resources.appIconOff;
            appTaskbarIcon.Text = string.Format("{0} | {1}", AppDomain.CurrentDomain.FriendlyName, connected ? "Verbunden" : "Keine Verbindung");
        }
        private void connectToServer()
        {
            try
            {
                client.Connect();
            }
            catch (Exception ex)
            {
                Logger.log.Error(ex);
            }
        }
        private void Client_ClientDisconnectedEvent(object obj, EventArgs e)
        {
            try
            {
                stopRecording();

                executeCodeOnUIThread(() =>
                {
                    setAppTaskbarIconState(false);
                    hideForm();
                });
            }
            catch (Exception ex)
            {
                Logger.log.Error(ex);
            }
        }
        private void Client_ClientConnectedEvent(object obj, ClientConnectedEventArgs e)
        {
            broadcastClient = e.BroadcastClient;

            executeCodeOnUIThread(() =>
            {
                setAppTaskbarIconState(true);
            });
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
            if (firstTimeShownTrayIcon == false)
            {
                firstTimeShownTrayIcon = true;
            }
            Activate();
        }
        private void executeCodeOnUIThread(Action code)
        {
            try
            {
                Invoke(code);
            }
            catch (Exception ex)
            {
                throw ex;
            }
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
            try
            {
                // start recording
                startRecording();
            }
            catch (Exception ex)
            {
                executeCodeOnUIThread(() =>
                {
                    MessageBoxManager.ShowMessageBoxError("Fehler bei der Aufnahme.\n\n\n" + ex.StackTrace);
                });
            }
        }
        private void btnRecord_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                stopRecording();
            }
            catch (Exception ex)
            {
                executeCodeOnUIThread(() =>
                {
                    MessageBoxManager.ShowMessageBoxError("Fehler bei der Aufnahme.\n\n\n" + ex.StackTrace);
                });
            }
        }
        private void startRecording()
        {
            btnRecord.Image = Properties.Resources.Speak_On;

            soundRecorder = new NAudioWrapper.Recorder(AppConfiguration.ReadConfig().CaptureDevice.Id);
            soundRecorder.RecordingStoppedEvent += SoundRecorder_RecordingStoppedEvent;

            startTimerRecordingDuration();
            soundRecorder.StartRecording();
        }
        private void stopRecording()
        {
            btnRecord.Image = Properties.Resources.Speak_Off;
            stopTimerRecordingDuration();

            soundRecorder?.StopRecording(); // data are in callback method ....
        }
        private void SoundRecorder_RecordingStoppedEvent(object obj, NAudioWrapper.RecordingStoppedEventArgs e)
        {
            // send voiceMessage ...
            try
            {
                client.SendVoiceMessage(new VoiceMessage(broadcastClient, e.SoundeData));
            }
            catch (Exception ex)
            {
                executeCodeOnUIThread(() =>
                {
                    MessageBoxManager.ShowMessageBoxError("Fehler beim übertragen der Broadcast-Nachricht.\n\n\n" + ex.StackTrace);
                });
            }
            finally
            {
                soundRecorder = null;
            }
        }

        // display the elapsed time from stopWatchSoundRecording
        private void timerDuration_Tick(object sender, EventArgs e)
        {
            timerRecordingSec++;
            lblSoundRecordingDuration.Text = new TimeSpan(0, 0, timerRecordingSec).ToString(@"hh\:mm\:ss");

            // reset
            if (timerRecordingSec >= 30) // max 30 sec recording
            {
                stopTimerRecordingDuration();

                stopRecording();
            }
        }
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason != CloseReason.ApplicationExitCall)
            {
                e.Cancel = true;
                hideForm();
            }
        }
        private void stopTimerRecordingDuration()
        {
            timerSoundRecordingDuration.Stop();
            timerRecordingSec = 0;

            executeCodeOnUIThread(() =>
            {
                lblSoundRecordingDuration.Text = "00:00:00";
                lblSoundRecordingDuration.Visible = false;
            });
        }
        private void startTimerRecordingDuration()
        {
            timerSoundRecordingDuration.Start();

            executeCodeOnUIThread(() =>
            {
                lblSoundRecordingDuration.Visible = true;
            });
        }
    }
}
