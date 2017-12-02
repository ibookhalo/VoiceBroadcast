using System;

namespace Network.Messaging
{
    [Serializable]
    public abstract class NetworkMessage
    {
        public static int MAX_DATA_SIZE_IN_BYTES = 1024 * 1024; // 1kb
    }
}