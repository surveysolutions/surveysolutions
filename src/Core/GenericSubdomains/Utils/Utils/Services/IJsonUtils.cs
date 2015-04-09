using System;
using System.IO;

namespace WB.Core.GenericSubdomains.Utils.Services
{
    public interface IJsonUtils
    {
        string Serialize(object item);
        string Serialize(object item, TypeSerializationSettings typeSerializationSettings);
        byte[] SerializeToByteArray(object item);
        void SerializeToStream(object value, Type type, Stream stream);
        T Deserialize<T>(string payload);
        T Deserialize<T>(byte[] payload);
        object DeserializeFromStream(Stream stream, Type type);
    }
}