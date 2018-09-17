using System;
using System.Collections.Generic;
using System.IO;

namespace WB.Services.Export.Services.Processing
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

        FileObject GetObjectMetadata(string key);
        bool IsExist(string path);
    }
}