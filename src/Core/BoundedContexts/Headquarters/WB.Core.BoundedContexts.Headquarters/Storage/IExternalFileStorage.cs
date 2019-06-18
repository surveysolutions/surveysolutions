using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.Storage
{
    class NoExternalFileSystemStorage : IExternalFileStorage
    {
        public bool IsEnabled()
        {
            return false;
        }

        public string GetDirectLink(string path, TimeSpan expiration)
        {
            throw new NotImplementedException();
        }

        public byte[] GetBinary(string key)
        {
            throw new NotImplementedException();
        }

        public List<FileObject> List(string prefix)
        {
            throw new NotImplementedException();
        }

        public void Remove(string path)
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

        public bool IsExist(string path)
        {
            throw new NotImplementedException();
        }
    }
}
