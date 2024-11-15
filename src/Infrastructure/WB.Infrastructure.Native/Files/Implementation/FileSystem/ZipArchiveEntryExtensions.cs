using System.IO;
using System.IO.Compression;

namespace WB.Infrastructure.Native.Files.Implementation.FileSystem;

public static class ZipArchiveEntryExtensions
{
    public static bool IsDirectory(this ZipArchiveEntry entry)
    {
        return Path.EndsInDirectorySeparator(entry.FullName);
    }
    
    public static byte[] GetContent(this ZipArchiveEntry entry)
    {
        using var entryStream = entry.Open();
        using var memoryStream = new MemoryStream();
        entryStream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }
}
