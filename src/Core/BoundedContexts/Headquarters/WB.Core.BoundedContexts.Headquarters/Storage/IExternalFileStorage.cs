using System;
using System.Collections.Generic;
using System.IO;

namespace WB.Core.BoundedContexts.Headquarters.Storage
{
    public interface IExternalFileStorage
    {
        bool IsEnabled();
        string GetDirectLink(string path, TimeSpan expiration);
        byte[] GetBinary(string key);
        List<FileObject> List(string prefix);

        void Remove(string path);

        FileObject Store(string path, byte[] data, string contentType, IProgress<int> progress = null);
        FileObject Store(string path, Stream inputStream, string contentType, IProgress<int> progress = null);

        bool IsExist(string path);
    }

    class NoExternalFileSystemStorage : IExternalFileStorage
    {
        public bool IsEnabled => false;
        
        bool IExternalFileStorage.IsEnabled()
        {
            throw new NotImplementedException();
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

        public bool IsExist(string path)
        {
            throw new NotImplementedException();
        }
    }
}