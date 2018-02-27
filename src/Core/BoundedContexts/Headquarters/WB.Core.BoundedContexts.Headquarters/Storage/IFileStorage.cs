using System;
using System.Collections.Generic;
using System.IO;

namespace WB.Core.BoundedContexts.Headquarters.Storage
{
    public interface IFileStorage
    {
        string GetDirectLink(string path, TimeSpan expiration);
        FileObject Store(string path, Stream inputStream, string contentType);
        byte[] GetBinary(string key);
        List<FileObject> List(string prefix);
        void Remove(string path);
        FileObject Store(string path, byte[] data, string contentType);
    }
}