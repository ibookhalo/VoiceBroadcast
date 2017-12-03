using System;

namespace Network.Messaging
{
    [Serializable]
    public abstract class NetworkMessage
    {
        public const int MAX_SIZE_BYTE = 5*1000*1000; // 5 MByte
    }
}