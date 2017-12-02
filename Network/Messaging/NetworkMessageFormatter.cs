
using  Network.Messaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace  Network.Messaging
{
    public class NetworkMessageFormatter<T> where T:NetworkMessage
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns>Liefert NULL, wenn die Deserialisierung fehlschlägt.</returns>
        public T Deserialize(byte[] data)
        {
            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                using (MemoryStream ms = new MemoryStream(data))
                {
                   return bf.Deserialize(ms) as T;
                }
                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

       
        public byte[] Serialize(T message)
        {
            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                using (MemoryStream ms = new MemoryStream())
                {
                    bf.Serialize(ms, message);
                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
