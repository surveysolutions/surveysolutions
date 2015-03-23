using System;
using System.IO;
using System.Text;
using WB.Core.GenericSubdomains.Utils.Services;

namespace WB.Core.GenericSubdomains.Utils.Rest
{
    public class ProtoBufSerializer : IJsonUtils
    {
        public string Serialize(object item)
        {
            return Encoding.UTF8.GetString(this.SerializeToByteArray(item));
        }

        public string Serialize(object item, TypeSerializationSettings typeSerializationSettings)
        {
            throw new NotImplementedException();
        }

        public byte[] SerializeToByteArray(object item)
        {
            using (var ms = new MemoryStream())
            {
                this.SerializeToStream(item, null, ms);
                return ms.ToArray();
            }
        }

        public void SerializeToStream(object value, Type type, Stream stream)
        {
            ProtoBuf.Serializer.NonGeneric.Serialize(stream, value);
        }

        public T Deserialize<T>(string payload)
        {
            return this.Deserialize<T>(Encoding.UTF8.GetBytes(payload ?? ""));
        }

        public T Deserialize<T>(byte[] payload)
        {
            return (T) this.DeserializeFromStream(new MemoryStream(payload), typeof (T));
        }

        public object DeserializeFromStream(Stream stream, Type type)
        {
            return ProtoBuf.Serializer.NonGeneric.Deserialize(type, stream);
        }
    }
}
