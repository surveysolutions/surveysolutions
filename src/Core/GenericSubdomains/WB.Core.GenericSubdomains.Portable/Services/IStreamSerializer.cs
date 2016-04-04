using System;
using System.IO;

namespace WB.Core.GenericSubdomains.Portable.Services
{
    public interface IStreamSerializer
    {
        void SerializeToStream(object value, Type type, Stream stream);
        object DeserializeFromStream(Stream stream, Type type, TypeSerializationSettings? typeSerializationSettings = null);
    }
}