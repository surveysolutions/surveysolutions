using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WB.Services.Export.Services.Processing;

namespace WB.Services.Export.Storage
{
    public interface IExternalFileStorage
    {
        bool IsEnabled();
        string GetDirectLink(string key, TimeSpan expiration, string asFilename = null);
        Task<byte[]> GetBinaryAsync(string key);
        Task<List<FileObject>> ListAsync(string prefix);

        Task RemoveAsync(string path);

        Task<FileObject> StoreAsync(string path, byte[] data, string contentType, IProgress<int> progress = null);
        Task<FileObject> StoreAsync(string path, Stream inputStream, string contentType, IProgress<int> progress = null);

        Task<FileObject> GetObjectMetadataAsync(string key);
        Task<bool> IsExistAsync(string path);
    }
}
