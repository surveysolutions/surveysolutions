using System.IO;
using Android.OS;
using WB.Core.SharedKernels.Enumerator.Services;
using Environment = System.Environment;

namespace WB.UI.Shared.Enumerator.Services
{
    public class AndroidPathUtils : IPathUtils
    {
        private readonly IPermissionsService permissionsService;

        public AndroidPathUtils(IPermissionsService permissionsService)
        {
            this.permissionsService = permissionsService;
        }

        public static string GetAndCreatePathToSubfolder(string workspace, string subFolderName)
        {
            var pathToSubfolderInLocalDirectory = Path.Combine(GetPathToInternalDirectory(), workspace, subFolderName);
            if (!Directory.Exists(pathToSubfolderInLocalDirectory))
            {
                Directory.CreateDirectory(pathToSubfolderInLocalDirectory);
            }

            return pathToSubfolderInLocalDirectory;
        }
        
        public static string GetPathToSubfolderInLocalDirectory(string subFolderName)
        {
            var pathToSubfolderInLocalDirectory = Path.Combine(GetPathToInternalDirectory(), subFolderName);
            if (!Directory.Exists(pathToSubfolderInLocalDirectory))
            {
                Directory.CreateDirectory(pathToSubfolderInLocalDirectory);
            }

            return pathToSubfolderInLocalDirectory;
        }

        public static string GetPathToInternalDirectory() => 
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        
        public static string GetPathToExternalDirectory() => 
            Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;

        public static string GetPathToInternalOrExternalDirectory()
        {
            var localApplicationDataDirectory = GetPathToInternalDirectory();
            return Directory.Exists(localApplicationDataDirectory)
                ? localApplicationDataDirectory
                : GetPathToExternalDirectory();
        }

        public static string GetPathToSubfolderInExternalDirectory(string subFolderName)
        {
            return Path.Combine(GetPathToExternalDirectory(), "Interviewer", subFolderName);
        }

        public static string GetPathToSupervisorSubfolderInExternalDirectory(string subFolderName)
        {
            return Path.Combine(GetPathToExternalDirectory(), "Supervisor", subFolderName);
        }

        public Task<string> GetRootDirectoryAsync()
        {
            return Task.FromResult(GetPathToInternalDirectory());
        }
    }
}
