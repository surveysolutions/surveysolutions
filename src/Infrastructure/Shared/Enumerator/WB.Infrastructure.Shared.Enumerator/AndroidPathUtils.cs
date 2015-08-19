using PCLStorage;

namespace WB.Infrastructure.Shared.Enumerator
{
    public class AndroidPathUtils
    {
        public static string GetPathToSubfolderInLocalDirectory(string subFolderName)
        {
            var pathToSubfolderInLocalDirectory = PortablePath.Combine(FileSystem.Current.LocalStorage.Path, subFolderName);

            var subfolderExistingStatus = FileSystem.Current.LocalStorage.CheckExistsAsync(pathToSubfolderInLocalDirectory).Result;
            if (subfolderExistingStatus != ExistenceCheckResult.FolderExists)
            {
                FileSystem.Current.LocalStorage.CreateFolderAsync(pathToSubfolderInLocalDirectory, CreationCollisionOption.FailIfExists).Wait();
            }

            return pathToSubfolderInLocalDirectory;
        }
    }
}