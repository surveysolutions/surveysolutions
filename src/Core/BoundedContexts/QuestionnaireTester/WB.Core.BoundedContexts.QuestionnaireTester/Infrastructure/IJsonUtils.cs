using System;
using System.IO;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure
{
    public interface IJsonUtils
    {
        Task<T> DeserializeAsync<T>(byte[] payload);
        Task<object> DeserializeFromStreamAsync(Stream stream, Type type);
        Task<byte[]> SerializeToByteArrayAsync(object payload);
    }
}
