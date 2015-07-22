using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Repositories
{
    internal class FilebasedPreloadedDataRepository : IPreloadedDataRepository
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IArchiveUtils archiveUtils;
        private readonly IRecordsAccessorFactory recordsAccessorFactory;
        private const string FolderName = "PreLoadedData";
        private const string UnzippedFoldername = "Unzipped";
        private readonly string path;
        private readonly string extension;
        private static ILogger Logger
        {
            get { return ServiceLocator.Current.GetInstance<ILogger>(); }
        }

        public FilebasedPreloadedDataRepository(IFileSystemAccessor fileSystemAccessor, string folderPath, IArchiveUtils archiveUtils, IRecordsAccessorFactory recordsAccessorFactory)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.archiveUtils = archiveUtils;
            this.recordsAccessorFactory = recordsAccessorFactory;

            this.path = fileSystemAccessor.CombinePath(folderPath, FolderName);
            if (!fileSystemAccessor.IsDirectoryExists(this.path))
                fileSystemAccessor.CreateDirectory(this.path);

            this.extension = ExportFileSettings.ExtensionOfExportedDataFile;
        }

        public string Store(Stream preloadedDataFile, string fileName)
        {
            var currentFolderId = Guid.NewGuid().FormatGuid();
            var currentFolderPath = fileSystemAccessor.CombinePath(path, currentFolderId);
            if (fileSystemAccessor.IsDirectoryExists(currentFolderPath))
                fileSystemAccessor.DeleteDirectory(currentFolderPath);

            fileSystemAccessor.CreateDirectory(currentFolderPath);
            using (var fileStream = fileSystemAccessor.OpenOrCreateFile(fileSystemAccessor.CombinePath(currentFolderPath, fileName),false))
            {
                preloadedDataFile.CopyTo(fileStream);
            }
            return currentFolderId;
        }

        public PreloadedContentMetaData GetPreloadedDataMetaInformationForSampleData(string id)
        {
            var filesInDirectory = GetFiles(id);
            var csvFilesInDirectory = filesInDirectory.Where(file => file.EndsWith(this.extension)).ToArray();
            if (csvFilesInDirectory.Length == 0)
                return null;

            var csvFile = csvFilesInDirectory[0];
            var csvFileName = fileSystemAccessor.GetFileName(csvFile);
            return new PreloadedContentMetaData(id, csvFileName,
                new[] { new PreloadedFileMetaData(csvFileName, fileSystemAccessor.GetFileSize(csvFile), true) }, PreloadedContentType.Sample);
        }

        public PreloadedContentMetaData GetPreloadedDataMetaInformationForPanelData(string id)
        {
            var filesInDirectory = GetFiles(id);
            var zipFilesInDirectory = filesInDirectory.Where(archiveUtils.IsZipFile).ToArray();
            if (zipFilesInDirectory.Length > 0)
            {
                try
                {
                    return new PreloadedContentMetaData(id,
                        fileSystemAccessor.GetFileName(zipFilesInDirectory[0]),
                        archiveUtils.GetArchivedFileNamesAndSize(zipFilesInDirectory[0])
                            .Select(file => new PreloadedFileMetaData(file.Key, file.Value, file.Key.EndsWith(this.extension)))
                            .ToArray(), PreloadedContentType.Panel);
                }
                catch (Exception e)
                {
                    Logger.Error(e.Message, e);
                    return null;
                }
            }
            return null;
        }

        public PreloadedDataByFile GetPreloadedDataOfSample(string id)
        {
            var filesInDirectory = GetFiles(id);
            var csvFilesInDirectory = filesInDirectory.Where(file => file.EndsWith(this.extension)).ToArray();
            if (csvFilesInDirectory.Length != 0)
            {
                return this.GetPreloadedDataFromFile(id, csvFilesInDirectory[0]);
            }
            return null;
        }

        public void DeletePreloadedDataOfSample(string id)
        {
            var currentFolderPath = fileSystemAccessor.CombinePath(path, id);
            
            if (fileSystemAccessor.IsDirectoryExists(currentFolderPath))
                fileSystemAccessor.DeleteDirectory(currentFolderPath);
        }

        public PreloadedDataByFile[] GetPreloadedDataOfPanel(string id)
        {
            var currentFolderPath = fileSystemAccessor.CombinePath(path, id);
            if (!fileSystemAccessor.IsDirectoryExists(currentFolderPath))
                return new PreloadedDataByFile[0];

            var filesInDirectory = fileSystemAccessor.GetFilesInDirectory(currentFolderPath);
            return this.TryToGetPreloadedDataFromZipArchive(filesInDirectory, id, currentFolderPath);
        }

        public void DeletePreloadedDataOfPanel(string id)
        {
            var currentFolderPath = fileSystemAccessor.CombinePath(path, id);

            if (fileSystemAccessor.IsDirectoryExists(currentFolderPath))
                fileSystemAccessor.DeleteDirectory(currentFolderPath);
        }

        private IEnumerable<string> GetFiles(string id)
        {
            var currentFolderPath = fileSystemAccessor.CombinePath(path, id);
            if (!fileSystemAccessor.IsDirectoryExists(currentFolderPath))
                return new string[0];

            return fileSystemAccessor.GetFilesInDirectory(currentFolderPath);
        }

        private PreloadedDataByFile[] TryToGetPreloadedDataFromZipArchive(IEnumerable<string> filesInDirectory, string id, string currentFolderPath)
        {
            var archivesInDirectory = filesInDirectory.Where(archiveUtils.IsZipFile).ToArray();
            if (archivesInDirectory.Length == 0)
                return new PreloadedDataByFile[0];

            var archivePath = archivesInDirectory[0];
            var unzippedDirectoryPath = fileSystemAccessor.CombinePath(currentFolderPath, UnzippedFoldername);

            if (!fileSystemAccessor.IsDirectoryExists(unzippedDirectoryPath))
            {
                try
                {
                    archiveUtils.Unzip(archivePath, unzippedDirectoryPath);
                }
                catch (Exception e)
                {
                    Logger.Error(e.Message, e);
                    return null;
                }
            }
            var unzippedFiles =
                fileSystemAccessor.GetFilesInDirectory(unzippedDirectoryPath).Where(filename => filename.EndsWith(this.extension)).ToArray();

            if (unzippedFiles.Length == 0)
            {
                var unzippedDirectories = fileSystemAccessor.GetDirectoriesInDirectory(unzippedDirectoryPath);
                if (unzippedDirectories == null || unzippedDirectories.Length == 0)
                    return new PreloadedDataByFile[0];

                unzippedFiles = fileSystemAccessor.GetFilesInDirectory(unzippedDirectories[0]);
            }

            return unzippedFiles
                .Where(filename => filename.EndsWith(this.extension))
                .Select(file => GetPreloadedDataFromFile(id, file))
                .Where(data => data != null)
                .ToArray();
        }

        private PreloadedDataByFile GetPreloadedDataFromFile(string id, string fileInDirectory)
        {
            string[] header = null;
            var records = new List<string[]>();
            try
            {
                using (var fileStream = fileSystemAccessor.ReadFile(fileInDirectory))
                {
                    var recordAccessor = recordsAccessorFactory.CreateRecordsAccessor(fileStream, ExportFileSettings.SeparatorOfExportedDataFile.ToString());

                    foreach (var record in recordAccessor.Records.ToList())
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
            return new PreloadedDataByFile(id, fileSystemAccessor.GetFileName(fileInDirectory), header?? new string[0], records.ToArray());
        }
    }
}
