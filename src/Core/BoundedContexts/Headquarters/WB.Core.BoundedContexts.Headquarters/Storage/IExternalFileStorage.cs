using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.Storage
{
    class NoExternalFileStorage : IExternalFileStorage
    {
        public bool IsEnabled()
        {
            return false;
        }

        public string GetDirectLink(string path, TimeSpan expiration)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> GetBinaryAsync(string key)
        {
            throw new NotImplementedException();
        }

        public Task<List<FileObject>> ListAsync(string prefix)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAsync(string path)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAsync(IEnumerable<string> paths)
        {
            throw new NotImplementedException();
        }

        public FileObject Store(string path, byte[] data, string contentType, IProgress<int> progress = null)
        {
            throw new NotImplementedException();
        }

        public FileObject Store(string path, Stream inputStream, string contentType, IProgress<int> progress = null)
        {
            throw new NotImplementedException();
        }

        public Task<FileObject> StoreAsync(string path, Stream inputStream, string contentType, IProgress<int> progress = null)
        {
            throw new NotImplementedException();
        }

        public FileObject GetObjectMetadata(string key)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsAsync(string path)
        {
            throw new NotImplementedException();
        }
    }
}
