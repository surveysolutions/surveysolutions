using System;
using System.IO;
using System.Deployment.Application;
using System.Windows.Interop;
using System.Reflection;

namespace DevExpress.RealtorWorld.DataService {
    public static class FilesHelper {
        static bool? isClickOnce;
        public static bool IsClickOnce {
            get {
                if(isClickOnce == null)
                    isClickOnce = !BrowserInteropHelper.IsBrowserHosted && ApplicationDeployment.IsNetworkDeployed;
                return (bool)isClickOnce;
            }
        }
        public static string DataDirectory {
            get {
                return IsClickOnce ? ApplicationDeployment.CurrentDeployment.DataDirectory : GetEntryAssemblyDirectory();
            }
        }
        static string GetEntryAssemblyDirectory() {
            Assembly curr = Assembly.GetEntryAssembly();
            if(curr == null) return null;
            string appPath = curr.Location;
            return Path.GetDirectoryName(appPath);
        }
        public static string DataPath { get { return "Data"; } }
        public static string FindFile(string fileName, string directoryName) {
            string appPath = DataDirectory;
            if(appPath == null) return null;
            string dirName = Path.GetFullPath(appPath);
            for(int n = 0; n < 9; n++) {
                string path = dirName + "\\" + directoryName + "\\" + fileName;
                try {
                    if(File.Exists(path))
                        return path;
                } catch { }
                dirName += @"\..";
            }
            throw new FileNotFoundException(fileName + " not found");
        }
        public static string FindDirectory(string directoryName) {
            string appPath = DataDirectory;
            if(appPath == null) return null;
            string dirName = Path.GetFullPath(appPath);
            for(int n = 0; n < 9; n++) {
                string path = dirName + "\\" + directoryName;
                try {
                    if(Directory.Exists(path))
                        return path;
                } catch { }
                dirName += @"\..";
            }
            throw new DirectoryNotFoundException(directoryName + " not found");
        }
    }
}
