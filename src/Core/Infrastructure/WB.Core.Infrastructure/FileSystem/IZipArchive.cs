using System;

namespace WB.Core.Infrastructure.FileSystem
{
    public interface IZipArchive : IDisposable
    {
        void CreateEntry(string path, byte[] content);
    }
}