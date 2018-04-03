using System;
using System.Text;

namespace WB.Core.Infrastructure.FileSystem
{
    public interface IZipArchive : IDisposable
    {
        void CreateEntry(string path, byte[] content);
    }

    public static class ZipArchiveExtensions
    {
        public static void PutTextEntry(this IZipArchive archive, string path, string content)
        {
            archive.CreateEntry(path, Encoding.UTF8.GetBytes(content));
        }
    }
}
