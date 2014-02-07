using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.BoundedContexts.Supervisor.Views.TabletInformation;

namespace WB.Core.BoundedContexts.Supervisor.Implementation.Services.TabletInformation
{
    internal class FileBasedTabletInformationService : ITabletInformationService
    {
        private readonly string basePath;
        private const string TabletInformationFolderName = "TabletInformationStorage";
        private const char Separator = '@';
        
        public FileBasedTabletInformationService(string parentFolder)
        {
            this.basePath = Path.Combine(parentFolder, TabletInformationFolderName);
            if (!Directory.Exists(this.basePath))
                Directory.CreateDirectory(this.basePath);
        }

        public void SaveTabletInformation(string packageName, byte[] content, string androidId, string registrationId)
        {
            var fileName = string.Format("{0}{1}{2}{1}{3}.zip", androidId, Separator, registrationId, DateTime.Now.Ticks);


            File.WriteAllBytes(GetFullPathToContentFile(fileName), content);
        }

        public List<TabletInformationView> GetAllTabletInformationPackages()
        {
            var result = new List<TabletInformationView>();
            var storageDirectory = new DirectoryInfo(basePath);
            
            foreach (var fileInfo in storageDirectory.GetFiles())
            {
                var fileName = Path.GetFileNameWithoutExtension(fileInfo.FullName);
                var separatedValues = fileName.Split(Separator);
                if(separatedValues.Length!=3)
                    continue;

                result.Add(new TabletInformationView(fileInfo.Name, separatedValues[0], separatedValues[1], fileInfo.CreationTime));
            }
            return result.OrderBy(r=>r.CreationDate).ToList();
        }

        public string GetFullPathToContentFile(string packageName)
        {
            return Path.Combine(basePath, packageName);
        }
    }
}
