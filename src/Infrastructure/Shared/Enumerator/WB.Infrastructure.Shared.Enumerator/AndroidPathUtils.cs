using System;
using System.IO;

namespace WB.Infrastructure.Shared.Enumerator
{
    public class AndroidPathUtils
    {
        public static string GetPathToSubfolderInLocalDirectory(string subFolderName)
            => GetPathToSubfolderInDirectory(GetPathToLocalDirectory(), subFolderName);

        public static string GetPathToLocalDirectory()
            => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        public static string GetPathToExternalDirectory()
            => Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;

        public static string GetPathToSubfolderInExternalDirectory(string subFolderName)
            => GetPathToSubfolderInDirectory(GetPathToExternalInterviewerDirectory(), subFolderName);

        public static string GetPathToExternalInterviewerDirectory()
            => Path.Combine(GetPathToExternalDirectory(), "Interviewer");

        public static string GetPathToFileInLocalSubDirectory(string subFolderName, string fileName)
            => GetPathToFileInSubDirectory(GetPathToSubfolderInLocalDirectory(subFolderName), fileName);

        public static string GetPathToFileInExternalSubDirectory(string subFolderName, string fileName)
            => GetPathToFileInSubDirectory(GetPathToSubfolderInExternalDirectory(subFolderName), fileName);

        private static string GetPathToSubfolderInDirectory(string directory, string subFolderName)
        {
            var pathToSubfolderInLocalDirectory = Path.Combine(directory, subFolderName);

            if(!Directory.Exists(pathToSubfolderInLocalDirectory))
                Directory.CreateDirectory(pathToSubfolderInLocalDirectory);

            return pathToSubfolderInLocalDirectory;
        }

        private static string GetPathToFileInSubDirectory(string subFolder, string fileName)
        {
            string filePath = Path.Combine(subFolder, fileName);
            if(!File.Exists(filePath))
                File.Create(filePath);
                
            return filePath;
        }
    }
}