using System;
using System.IO;

namespace WB.Core.GenericSubdomains.Portable.Services
{
    public interface IJsonAllTypesSerializer
    {
        string Serialize(object item);
        byte[] SerializeToByteArray(object item);

        T Deserialize<T>(byte[] payload);
        T Deserialize<T>(string payload);
        T Deserialize<T>(string payload, Type payloadType);
        object DeserializeFromStream(Stream stream, Type type);
    }
}
