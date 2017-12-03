using System;


namespace Network
{
    [Serializable]
    public class BroadCastClient
    {
        public string Name { private set; get; }
        public uint? Id { private set; get; }
        public BroadCastClient(string clientName, uint? clientId)
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
