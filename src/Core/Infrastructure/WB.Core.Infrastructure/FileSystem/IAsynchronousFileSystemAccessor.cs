using System.Threading.Tasks;

namespace WB.Core.Infrastructure.FileSystem
{
    public interface IAsynchronousFileSystemAccessor
    {
        Task WriteAllBytesAsync(string pathToFile, byte[] content);
        Task CopyFileAsync(string sourceDir, string targetDir);
        Task CreateDirectoryAsync(string path);

        Task<bool> IsDirectoryExistsAsync(string pathToDirectory);
        Task<bool> IsFileExistsAsync(string pathToFile);

        string CombinePath(string path1, string path2);
    }
}