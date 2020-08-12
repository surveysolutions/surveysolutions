using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.Services.Processing;

namespace WB.Services.Export.Storage
{
    public interface IExternalArtifactsStorage
    {
        bool IsEnabled();
        string GetDirectLink(string key, TimeSpan expiration, string? asFilename = null);
        Task<byte[]?> GetBinaryAsync(string key);
        Task<List<FileObject>?> ListAsync(string prefix);

        Task RemoveAsync(string path);

        Task<FileObject> StoreAsync(string path, byte[] data, string contentType, ExportProgress? progress = null,
            CancellationToken cancellationToken = default);
        Task<FileObject> StoreAsync(string path, Stream inputStream, string contentType, 
            ExportProgress? progress = null, CancellationToken cancellationToken = default);

        Task<FileObject?> GetObjectMetadataAsync(string key);
        Task<bool> IsExistAsync(string path);
    }
}
