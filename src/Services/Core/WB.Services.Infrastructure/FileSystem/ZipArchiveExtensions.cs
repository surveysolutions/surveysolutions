using System.Text;

namespace WB.Services.Infrastructure.FileSystem
{
    public static class ZipArchiveExtensions
    {
        public static void PutTextEntry(this IZipArchive archive, string path, string content)
        {
            archive.CreateEntry(path, Encoding.UTF8.GetBytes(content));
        }
    }
}