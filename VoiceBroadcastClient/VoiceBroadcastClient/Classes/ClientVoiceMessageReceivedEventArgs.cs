using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Network.Messaging;

namespace VoiceBroadcastClient.Classes
{
    class ClientVoiceMessageReceivedEventArgs : EventArgs
    {
        public VoiceMessage VoiceMessage { get; private set; }

        public ClientVoiceMessageReceivedEventArgs(VoiceMessage voiceMessage)
        {
            this.VoiceMessage = voiceMessage;
        }
    }
}
