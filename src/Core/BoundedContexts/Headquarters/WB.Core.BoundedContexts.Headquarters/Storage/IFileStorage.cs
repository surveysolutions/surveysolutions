using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Storage
{
    public interface IFileStorage
    {
        byte[] GetBinary(string key);
        List<FileObject> List(string prefix);
        void Remove(string path);
        FileObject Store(string path, byte[] data, string contentType);
    }
}