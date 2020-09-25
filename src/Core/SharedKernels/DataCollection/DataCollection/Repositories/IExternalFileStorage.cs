#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.DataCollection.Repositories
{
    public interface IExternalFileStorage
    {
        bool IsEnabled();

        string GetDirectLink(string path, TimeSpan expiration);

        Task<byte[]?> GetBinaryAsync(string key);

        Task<List<FileObject>?> ListAsync(string prefix);

        Task RemoveAsync(string path);
        Task RemoveAsync(IEnumerable<string> paths);

        FileObject Store(string path, byte[] data, string contentType, IProgress<int>? progress = null);

        FileObject Store(string path, Stream inputStream, string contentType, IProgress<int>? progress = null);

        Task<FileObject> StoreAsync(string path, Stream inputStream, string contentType, IProgress<int>? progress = null);
    }
}
