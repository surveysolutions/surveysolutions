using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace WB.Services.Infrastructure.FileSystem
{
    public interface IZipArchive : IDisposable
    {
        void CreateEntry(string path, byte[] content);
        void CreateEntry(string path, Stream content);
        Task CreateEntryAsync(string path, byte[] content, CancellationToken token = default);
        Task CreateEntryAsync(string path, Stream content, CancellationToken token = default);
    }
}
