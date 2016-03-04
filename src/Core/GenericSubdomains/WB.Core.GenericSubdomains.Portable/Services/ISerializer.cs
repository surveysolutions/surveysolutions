using System;
using System.IO;

namespace WB.Core.GenericSubdomains.Portable.Services
{
    public interface ISerializer
    {
        string Serialize(object item);
        string Serialize(object item, TypeSerializationSettings typeSerializationSettings);
        string Serialize(object item, SerializationBinderSettings binderSettings);

        byte[] SerializeToByteArray(object item);
        byte[] SerializeToByteArray(object item, TypeSerializationSettings typeSerializationSettings);
        void SerializeToStream(object value, Type type, Stream stream);

        T Deserialize<T>(string payload);
        T Deserialize<T>(byte[] payload);
        T Deserialize<T>(string payload, TypeSerializationSettings typeSerializationSettings);

        object Deserialize(byte[] payload, Type objectType, TypeSerializationSettings typeSerializationSettings);

        object Deserialize(string payload, Type type, TypeSerializationSettings typeSerializationSettings);
        object DeserializeFromStream(Stream stream, Type type, TypeSerializationSettings? typeSerializationSettings = null);
    }
}