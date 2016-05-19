using PCLStorage;

namespace WB.Infrastructure.Shared.Enumerator
{
    public class AndroidPathUtils
    {
        public static string GetPathToSubfolderInLocalDirectory(string subFolderName)
        {
            return GetPathToSubfolderInDirectory(GetPathToLocalDirectory(), subFolderName);
        }

        public static string GetPathToLocalDirectory()
        {
            return FileSystem.Current.LocalStorage.Path;
        }

        public static string GetPathToExternalDirectory()
        {
            return Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
        }

        public static string GetPathToSubfolderInExternalDirectory(string subFolderName)
        {
            return GetPathToSubfolderInDirectory(GetPathToExternalInterviewerDirectory(), subFolderName);
        }

        public static string GetPathToExternalInterviewerDirectory()
        {
            return PortablePath.Combine(GetPathToExternalDirectory(), "Interviewer");
        }

        public static string GetPathToFileInLocalSubDirectory(string subFolderName, string fileName)
        {
            return GetPathToFileInSubDirectory(GetPathToSubfolderInLocalDirectory(subFolderName), fileName);
        }

        public static string GetPathToFileInExternalSubDirectory(string subFolderName, string fileName)
        {
            return GetPathToFileInSubDirectory(GetPathToSubfolderInExternalDirectory(subFolderName), fileName);
        }

        public static string GetPathToLogFile()
        {
            return GetPathToFileInSubDirectory(GetPathToSubfolderInExternalDirectory("Logs"), "interviewer.log");
        }

        private static string GetPathToSubfolderInDirectory(string directory, string subFolderName)
        {
            var pathToSubfolderInLocalDirectory = PortablePath.Combine(directory, subFolderName);

            var subfolderExistingStatus =
                FileSystem.Current.LocalStorage.CheckExistsAsync(pathToSubfolderInLocalDirectory).Result;
            if (subfolderExistingStatus != ExistenceCheckResult.FolderExists)
            {
                FileSystem.Current.LocalStorage.CreateFolderAsync(pathToSubfolderInLocalDirectory,
                    CreationCollisionOption.FailIfExists).Wait();
            }

            return pathToSubfolderInLocalDirectory;
        }

        private static string GetPathToFileInSubDirectory(string  subFolder, string fileName)
        {
            return FileSystem.Current.GetFolderFromPathAsync(subFolder)
                .Result
                .CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists)
                .Result
                .Path;
        }
    }
}