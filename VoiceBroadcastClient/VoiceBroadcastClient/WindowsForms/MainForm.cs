using NAudioWrapper;
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

        private SoundRecorder soundRecorder;
        private SoundPlayer soundPlayer;

        private Queue<VoiceMessage> voiceMessageQueue;

        private const string MESSAGE_NO_CAPTUREDEVICE_FOUND_STRING = "Voicebroadcast konnte kein Aufnahmegerät (Mikrofon) finden!\nBitte überprüfen Sie die Einstellungen.";
        private const string ERROR_PLAYING_VOICEMESSAGE_STRING = "Fehler beim Abspielen der Broadcast-Nachricht.";

        public MainForm()
        {
            InitializeComponent();

            voiceMessageQueue = new Queue<VoiceMessage>();

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

            // new balloon ...
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
                try
                {
                    int renderDeviceId = AppConfiguration.ReadConfig().RenderDevice.Id;

                    if (renderDeviceId >= 0)
                    {
                        lock (voiceMessageQueue)
                        {
                            voiceMessageQueue.Enqueue(e.VoiceMessage);
                            if (soundPlayer == null)
                            {
                                soundPlayer = new SoundPlayer(renderDeviceId);
                                soundPlayer.PlaybackStoppedEvent += soundPlayer_PlaybackStopped;

                                showVoiceMessageReceivedBallonTip(e.VoiceMessage);
                                soundPlayer.Play(voiceMessageQueue.Dequeue().Data);
                            }
                        }
                    }/*
                    else
                    {
                        MessageBoxManager.ShowMessageBoxError("Fehler beim Abspielen der Broadcast-Nachricht, da kein Ausgabegerät gefunden werden konnte.");
                    }*/
                }
                catch (Exception ex)
                {
                    MessageBoxManager.ShowMessageBoxError(ERROR_PLAYING_VOICEMESSAGE_STRING);
                    Logger.log.Error(ex);
                }
            });
        }
        private void soundPlayer_PlaybackStopped(object sender, EventArgs e)
        {
            lock (voiceMessageQueue)
            {
                if (voiceMessageQueue.Count > 0) // voicemessage data are in queue
                {
                    VoiceMessage voiceMessage = voiceMessageQueue.Dequeue();

                    executeCodeOnUIThread(() => {
                        showVoiceMessageReceivedBallonTip(voiceMessage);
                        try
                        {
                            soundPlayer.Play(voiceMessage.Data);
                        }
                        catch (Exception ex)
                        {
                            Logger.log.Error(ex);
                            MessageBoxManager.ShowMessageBoxError(ERROR_PLAYING_VOICEMESSAGE_STRING);
                        }
                    });
                }
                else
                {
                    soundPlayer.Stop();
                    soundPlayer = null;
                }
            }
        }
        private void TrayIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int captureDeviceId = getActiveCaptureDeviceId();
                if (!client.IsConnected)
                {
                    MessageBoxManager.ShowMessageBoxError("Voicebroadcast hat keine Verbindung zum Server!");
                }
                else if (captureDeviceId >= 0)
                {
                    setAppTaskbarIconState(true);
                    showForm();
                }
                else
                {
                    MessageBoxManager.ShowMessageBoxError(MESSAGE_NO_CAPTUREDEVICE_FOUND_STRING);
                }
            }
        }

        /*private int getActiveRenderDeviceId()
        {
            // read config
            var conf = AppConfiguration.ReadConfig();

            // read active render devices
            var renderDevices = new AudioDeviceEnemerator().GetRenderDevices();

            DeviceInfo device = new DeviceInfo();

            // is config ok?
            if (renderDevices.Exists(cd => cd.Id.Equals(conf.RenderDevice.Id)))
            {
                device = conf.RenderDevice;
                return device.Id;
            }
            else if (renderDevices.Count > 0)
            {
                device = renderDevices.First();
            }
            conf.RenderDevice = device;

            try
            {
                AppConfiguration.SaveConfig(conf);
            }
            catch (Exception ex)
            {
                Logger.log.Warn(ex);
            }

            return device.Id;
        }
        */
        private int getActiveCaptureDeviceId()
        {
            // read config
            var conf = AppConfiguration.ReadConfig();

            // read active capture devices
            var captureDevices = new AudioDeviceEnemerator().GetCaptureDevices();

            DeviceInfo device = new DeviceInfo();

            // is config ok?
            if (captureDevices.Exists(cd=>cd.Id.Equals(conf.CaptureDevice.Id)))
            {
                device = conf.CaptureDevice;
                return device.Id;
            }
            else if (captureDevices.Count>0)
            {
                device = captureDevices.First();
            }
            conf.CaptureDevice = device;

            try
            {
                AppConfiguration.SaveConfig(conf);
            }
            catch (Exception ex)
            {
                Logger.log.Warn(ex);
            }

            return device.Id;
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
            executeCodeOnUIThread(() =>
            {
                stopRecording();
                setAppTaskbarIconState(false);
                hideForm();
            });
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
            if (configForm==null || (!configForm.Visible && configForm.IsDisposed))
            {
                configForm = new ConfigForm();
                configForm.Show();
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
            int captureDeviceId = getActiveCaptureDeviceId();
            if (captureDeviceId>=0)
            {
                try
                {
                    btnRecord.Image = Properties.Resources.Speak_Off;

                    soundRecorder = new SoundRecorder(captureDeviceId);
                    soundRecorder.RecordingStoppedEvent += SoundRecorder_RecordingStoppedEvent;

                    startTimerRecordingDuration();
                    soundRecorder.Start();
                }
                catch (Exception ex)
                {
                    stopTimerRecordingDuration();
                    soundRecorder = null;
                    throw ex;
                }
            }
            else
            {
                MessageBoxManager.ShowMessageBoxError(MESSAGE_NO_CAPTUREDEVICE_FOUND_STRING);
            }
        }
        private void stopRecording()
        {
            btnRecord.Image = Properties.Resources.Speak_On;
            stopTimerRecordingDuration();

            soundRecorder?.Stop(); // data are in callback method ....
        }
        private void SoundRecorder_RecordingStoppedEvent(object obj, NAudioWrapper.SoundRecordingStoppedEventArgs e)
        {
            if (e.SoundeData.Length >= 10000 /*more than ~ 500 ms of recording time*/)
            {
                if (client.IsConnected)
                {
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
                }
                else
                {
                    executeCodeOnUIThread(() =>
                    {
                        MessageBoxManager.ShowMessageBoxError("Ihre Broadcast-Nachricht konnte nicht versendet werden, da es keine Verbindung zum Server besteht.");
                    });
                }
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
