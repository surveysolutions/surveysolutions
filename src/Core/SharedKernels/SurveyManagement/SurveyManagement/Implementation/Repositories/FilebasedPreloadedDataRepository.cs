using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using Ionic.Zip;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Implementation.SampleRecordsAccessors;
using WB.Core.SharedKernels.SurveyManagement.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Repositories
{
    internal class FilebasedPreloadedDataRepository : IPreloadedDataRepository
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private const string FolderName = "PreLoadedData";
        private readonly string csvExtension = ".csv";
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

        public PreloadedContentMetaData GetPreloadedDataMetaInformation(Guid id)
        {
             var currentFolderPath = fileSystemAccessor.CombinePath(path, id.FormatGuid());
            if (!fileSystemAccessor.IsDirectoryExists(currentFolderPath))
                return null;

            var filesInDirectory = fileSystemAccessor.GetFilesInDirectory(currentFolderPath).Where(ZipFile.IsZipFile).ToArray();
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
                        if (!fileName.EndsWith(csvExtension))
                            continue;

                        files.Add(new PreloadedFileMetaData(zip.FileName, zip.UncompressedSize));
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message, e);
                return null;
            }
            return new PreloadedContentMetaData(id,
                fileSystemAccessor.GetFileName(filesInDirectory[0]), files.ToArray());
        }

        public PreloadedDataByFile[] GetPreloadedData(Guid id)
        {
            var currentFolderPath = fileSystemAccessor.CombinePath(path, id.FormatGuid());
            if (!fileSystemAccessor.IsDirectoryExists(currentFolderPath))
                return new PreloadedDataByFile[0];

            var archivesInDirectory = fileSystemAccessor.GetFilesInDirectory(currentFolderPath).Where(ZipFile.IsZipFile).ToArray();
            if (archivesInDirectory.Length == 0)
                return null;
            var archivePath = archivesInDirectory[0];
            var unzippedDirectoryPath = archivePath.Substring(0, archivePath.LastIndexOf('.'));
            if (!fileSystemAccessor.IsDirectoryExists(unzippedDirectoryPath))
            {
                try
                {
                    using (var zips = ZipFile.Read(archivePath))
                    {
                        zips.ExtractAll(unzippedDirectoryPath, ExtractExistingFileAction.OverwriteSilently);
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e.Message, e);
                    return null;
                }
            }
            return
                fileSystemAccessor.GetFilesInDirectory(unzippedDirectoryPath)
                    .Where(filename => filename.EndsWith(csvExtension))
                    .Select(file => GetPreloadedDataFromFile(id, file))
                    .Where(data => data != null)
                    .ToArray();
        }

        private PreloadedDataByFile GetPreloadedDataFromFile(Guid id,string fileInDirectory)
        {
            string[] header = null;
            var records = new List<string[]>();
            try
            {
                using (var fileStream = new FileStream(fileInDirectory, FileMode.Open))
                {
                    var recordAccessor = new CsvSampleRecordsAccessor(fileStream);

                    foreach (var record in recordAccessor.Records)
                    {
                        if (header == null)
                        {
                            header = record;
                            continue;
                        }
                        records.Add(record);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message, e);
                return null;
            }
            return new PreloadedDataByFile(id, fileSystemAccessor.GetFileName(fileInDirectory), header, records.ToArray());
        }
    }
}
