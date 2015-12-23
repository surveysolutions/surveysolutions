using System.IO;
using System.Threading.Tasks;
using WB.Core.Infrastructure.FileSystem;
using PCLStorage;

namespace WB.Infrastructure.Shared.Enumerator.Internals.FileSystem
{
    internal class AsynchronousFileSystemAccessor: IAsynchronousFileSystemAccessor
    {
        public async Task WriteAllBytesAsync(string pathToFile, byte[] content)
        {
            var parentFolderPath = Directory.GetParent(pathToFile).FullName;
            var parentFolder = await PCLStorage.FileSystem.Current.GetFolderFromPathAsync(parentFolderPath);

            var emptyBackupFile = await parentFolder.CreateFileAsync(Path.GetFileName(pathToFile), CreationCollisionOption.GenerateUniqueName);

            using (var stream = await emptyBackupFile.OpenAsync(PCLStorage.FileAccess.ReadAndWrite))
            {
                stream.Write(content, 0, content.Length);
            }
        }

        public async Task CopyFileAsync(string sourceFile, string targetDir)
        {
            var parentFolder = await PCLStorage.FileSystem.Current.GetFolderFromPathAsync(targetDir);

            var copyOfTheFile = await parentFolder.CreateFileAsync(Path.GetFileName(sourceFile), CreationCollisionOption.GenerateUniqueName);
            var originalFile = await PCLStorage.FileSystem.Current.GetFileFromPathAsync(sourceFile);

            using (var originalStream = await originalFile.OpenAsync(PCLStorage.FileAccess.Read))
            {
                using (var copyStream = await copyOfTheFile.OpenAsync(PCLStorage.FileAccess.ReadAndWrite))
                {
                    await originalStream.CopyToAsync(copyStream);
                }
            }
        }

        public async Task CreateDirectoryAsync(string path)
        {
            var parentFolderPath = Directory.GetParent(path).FullName;
            var parentFolder = await PCLStorage.FileSystem.Current.GetFolderFromPathAsync(parentFolderPath);
            await parentFolder.CreateFolderAsync(Path.GetDirectoryName(path), CreationCollisionOption.FailIfExists);
        }

        public async Task<bool> IsDirectoryExistsAsync(string pathToDirectory)
        {
            var parentFolderPath = Directory.GetParent(pathToDirectory).FullName;
            var parentFolder = await PCLStorage.FileSystem.Current.GetFolderFromPathAsync(parentFolderPath);
            return (await parentFolder.CheckExistsAsync(Path.GetDirectoryName(pathToDirectory))) ==
                   ExistenceCheckResult.FolderExists;
        }

        public async Task<bool> IsFileExistsAsync(string pathToFile)
        {
            var parentFolderPath = Directory.GetParent(pathToFile).FullName;
            var parentFolder = await PCLStorage.FileSystem.Current.GetFolderFromPathAsync(parentFolderPath);

            return (await parentFolder.CheckExistsAsync(Path.GetFileName(pathToFile))) ==
                  ExistenceCheckResult.FileExists;
        }

        public string CombinePath(string path1, string path2)
        {
            return PortablePath.Combine(path1, path2);
        }
    }
}