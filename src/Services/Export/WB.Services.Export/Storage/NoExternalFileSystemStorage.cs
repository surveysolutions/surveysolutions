﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.Services.Processing;

namespace WB.Services.Export.Storage
{
    class NoExternalArtifactsSystemStorage : IExternalArtifactsStorage
    {
        public bool IsEnabled()
        {
            return false;
        }

        public string GetDirectLink(string key, TimeSpan expiration, string? asFilename = null) 
            => throw new NotImplementedException();

        public Task<byte[]?> GetBinaryAsync(string key) => throw new NotImplementedException();
        public Task<List<FileObject>?> ListAsync(string prefix) => throw new NotImplementedException();
        public Task RemoveAsync(string path) => throw new NotImplementedException();

        public Task<FileObject> StoreAsync(string path, byte[] data, string contentType, ExportProgress? progress = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<FileObject> StoreAsync(string path, byte[] data, string contentType, ExportProgress? progress = null) => throw new NotImplementedException();
        public Task<FileObject> StoreAsync(string path, Stream inputStream, string contentType,
            ExportProgress? progress = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<FileObject?> GetObjectMetadataAsync(string key) => throw new NotImplementedException();
        public Task<bool> IsExistAsync(string path) => throw new NotImplementedException();
    }
}
