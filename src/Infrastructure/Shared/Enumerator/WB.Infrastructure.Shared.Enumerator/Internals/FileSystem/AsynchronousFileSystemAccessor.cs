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

        public async Task DeleteFileAsync(string pathTofile)
        {
            var file = await PCLStorage.FileSystem.Current.GetFileFromPathAsync(pathTofile);
            await file.DeleteAsync();
        }

        public async Task RemoveDirectoryAsync(string path)
        {
            var folder = await PCLStorage.FileSystem.Current.GetFolderFromPathAsync(path);
            await folder.DeleteAsync();
        }

        public async Task CopyFileAsync(string sourceFile, string targetDir)
        {
            var parentFolder = await PCLStorage.FileSystem.Current.GetFolderFromPathAsync(targetDir);
            var copyOfTheFile = await parentFolder.CreateFileAsync(Path.GetFileName(sourceFile), CreationCollisionOption.ReplaceExisting);
            var originalFile = await PCLStorage.FileSystem.Current.GetFileFromPathAsync(sourceFile);

            using (var originalStream = await originalFile.OpenAsync(PCLStorage.FileAccess.Read))
            {
                using (var copyStream = await copyOfTheFile.OpenAsync(PCLStorage.FileAccess.ReadAndWrite))
                {
                    await originalStream.CopyToAsync(copyStream);
                }
            }
        }

        public async Task CopyDirectoryAsync(string sourceDirectory, string targetDir)
        {
            var sourceFolder = await PCLStorage.FileSystem.Current.GetFolderFromPathAsync(sourceDirectory);
            if (sourceFolder != null)
            {
                var targetFolderPath = Path.Combine(targetDir, sourceFolder.Name);
                var targetFolder = await PCLStorage.FileSystem.Current.GetFolderFromPathAsync(targetFolderPath);

                if (targetFolder == null)
                {
                    await CreateDirectoryAsync(targetFolderPath);
                }

                var filesInSourceFolder = await sourceFolder.GetFilesAsync();

                foreach (var file in filesInSourceFolder)
                {
                    await CopyFileAsync(file.Path, targetFolderPath);
                }
            }
        }

        public async Task CreateDirectoryAsync(string path)
        {
            string parentFolderPath = path;
            IFolder parentFolder;

            do
            {
                parentFolderPath = Directory.GetParent(parentFolderPath).FullName;
                parentFolder = await PCLStorage.FileSystem.Current.GetFolderFromPathAsync(parentFolderPath);
            } while (parentFolder == null);

            var subPathInExistingFolderToCreate = path.Replace(parentFolderPath, "").TrimStart(Path.DirectorySeparatorChar);
            await parentFolder.CreateFolderAsync(subPathInExistingFolderToCreate, CreationCollisionOption.FailIfExists);
        }

        public async Task<bool> IsDirectoryExistsAsync(string pathToDirectory)
        {
            var parentFolder = await PCLStorage.FileSystem.Current.GetFolderFromPathAsync(pathToDirectory);
            return parentFolder != null;
        }

        public async Task<bool> IsFileExistsAsync(string pathToFile)
        {
            var parentFolderPath = Directory.GetParent(pathToFile).FullName;
            var parentFolder = await PCLStorage.FileSystem.Current.GetFolderFromPathAsync(parentFolderPath);
            if (parentFolder == null) return false;
            
            var checkResult = await parentFolder.CheckExistsAsync(Path.GetFileName(pathToFile));
            return checkResult == ExistenceCheckResult.FileExists;
        }

        public string CombinePath(string path1, string path2)
        {
            return PortablePath.Combine(path1, path2);
        }
    }
}