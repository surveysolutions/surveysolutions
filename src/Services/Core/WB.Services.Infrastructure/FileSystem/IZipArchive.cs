using System;
using System.IO;

namespace WB.Services.Infrastructure.FileSystem
{
    public interface IZipArchive : IDisposable
    {
        void CreateEntry(string path, byte[] content);
        void CreateEntry(string path, Stream content);
    }
}
