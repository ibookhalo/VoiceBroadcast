using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network
{
    [Serializable]
    public class BroadCastClient
    {
        public string ClientName { private set; get; }
        public int ClientId { private set; get; }

        public BroadCastClient(string clientName, int clientId)
        {
            this.ClientName = clientName;
            this.ClientId = clientId;
        }
    }

}
