using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ionic.Zip;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Repositories
{
    internal class FilebasedPreloadedDataRepository : IPreloadedDataRepository
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private const string FolderName = "PreLoadedData";
        private readonly string csvExtension = ".csv";
        private readonly string zipExtension = ".zip";
        private readonly string path;
        private static ILogger Logger
        {
            get { return ServiceLocator.Current.GetInstance<ILogger>(); }
        }

        public FilebasedPreloadedDataRepository(IFileSystemAccessor fileSystemAccessor, string folderPath)
        {
            this.fileSystemAccessor = fileSystemAccessor;

            this.path = fileSystemAccessor.CombinePath(folderPath, FolderName);
            if (!fileSystemAccessor.IsDirectoryExists(this.path))
                fileSystemAccessor.CreateDirectory(this.path);
        }

        public Guid Store(Stream preloadedDataFile, string fileName)
        {
            var currentFolderId = Guid.NewGuid();
            var currentFolderPath = fileSystemAccessor.CombinePath(path, currentFolderId.FormatGuid());
            if (fileSystemAccessor.IsDirectoryExists(currentFolderPath))
                fileSystemAccessor.DeleteDirectory(currentFolderPath);

            fileSystemAccessor.CreateDirectory(currentFolderPath);
            using (var fileStream = fileSystemAccessor.OpenOrCreateFile(fileSystemAccessor.CombinePath(currentFolderPath, fileName)))
            {
                preloadedDataFile.CopyTo(fileStream);
            }
            return currentFolderId;
        }

        public PreloadedDataMetaData GetPreloadedDataMetaInformation(Guid id)
        {
             var currentFolderPath = fileSystemAccessor.CombinePath(path, id.FormatGuid());
            if (!fileSystemAccessor.IsDirectoryExists(currentFolderPath))
                return null;

            var filesInDirectory = fileSystemAccessor.GetFilesInDirectory(currentFolderPath).Where(file => file.Contains(zipExtension)).ToArray();
            if (filesInDirectory.Length == 0)
                return null;

            var files = new List<PreloadedFileMetaData>();
            try
            {
                using (var zips = ZipFile.Read(filesInDirectory[0]))
                {
                    foreach (var zip in zips)
                    {
                        var fileName = zip.FileName;
                        if (!fileName.Contains(csvExtension))
                            continue;

                        files.Add(new PreloadedFileMetaData(zip.FileName, zip.UncompressedSize));
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message, e);
            }
            return new PreloadedDataMetaData(id,
                fileSystemAccessor.GetFileName(filesInDirectory[0]), files.ToArray());
        }
    }
}
