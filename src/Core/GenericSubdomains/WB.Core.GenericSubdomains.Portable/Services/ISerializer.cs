using System;
using System.IO;

namespace WB.Core.GenericSubdomains.Portable.Services
{
    public interface ISerializer
    {
        string Serialize(object item);
        
        void SerializeToStream(object value, Type type, Stream stream);

        T Deserialize<T>(string payload);
    }
}