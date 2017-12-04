using System;


namespace Network
{
    [Serializable]
    public class BroadcastClient
    {
        public string Name { private set; get; }
        public uint? Id { private set; get; }
        public BroadcastClient(string clientName, uint? clientId)
        {
            this.Name = clientName;
            this.Id = clientId;
        }

        public override string ToString()
        {
            return $"Clientname: {Name}, ClientID: {Id}";
        }
    }

}
