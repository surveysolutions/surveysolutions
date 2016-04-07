using System;
using System.IO;

namespace WB.Core.GenericSubdomains.Portable.Services
{
    [Obsolete("Since v. 5.7")]
    public interface IProtobufSerializer
    {
        void SerializeToStream(object value, Type type, Stream stream);
        object DeserializeFromStream(Stream stream, Type type);
    }
}