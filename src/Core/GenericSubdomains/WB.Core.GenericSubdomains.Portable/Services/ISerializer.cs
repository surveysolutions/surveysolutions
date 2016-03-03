using System;
using System.IO;

namespace WB.Core.GenericSubdomains.Portable.Services
{
    public interface ISerializer
    {
        string Serialize(object item);
        string Serialize(object item, TypeSerializationSettings typeSerializationSettings);

        byte[] SerializeToByteArray(object item);
        byte[] SerializeToByteArray(object item, TypeSerializationSettings typeSerializationSettings, SerializationType serializationType);
        void SerializeToStream(object value, Type type, Stream stream);

        T Deserialize<T>(string payload);
        T Deserialize<T>(byte[] payload);
        T Deserialize<T>(string payload, TypeSerializationSettings typeSerializationSettings);

        object Deserialize(byte[] payload, Type objectType, TypeSerializationSettings typeSerializationSettings,
            SerializationType serializationType);

        object Deserialize(string payload, Type type, TypeSerializationSettings typeSerializationSettings);
        object DeserializeFromStream(Stream stream, Type type, TypeSerializationSettings? typeSerializationSettings = null);
    }
}