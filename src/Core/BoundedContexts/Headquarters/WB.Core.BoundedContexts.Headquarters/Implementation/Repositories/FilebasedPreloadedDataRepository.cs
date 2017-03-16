using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.Export;
using WB.Core.BoundedContexts.Headquarters.Views.PreloadedData;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Repositories
{
    internal class FilebasedPreloadedDataRepository : IPreloadedDataRepository
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IArchiveUtils archiveUtils;
        private readonly IRecordsAccessorFactory recordsAccessorFactory;
        private const string FolderName = "PreLoadedData";
        private const string UnzippedFoldername = "Unzipped";
        private readonly string path;
        private readonly string[] permittedFileExtensions = { ExportFileSettings.DataFileExtension, ".txt" };
        private static ILogger Logger => ServiceLocator.Current.GetInstance<ILoggerProvider>().GetFor<FilebasedPreloadedDataRepository>();

        public FilebasedPreloadedDataRepository(IFileSystemAccessor fileSystemAccessor, string folderPath, IArchiveUtils archiveUtils, IRecordsAccessorFactory recordsAccessorFactory)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.archiveUtils = archiveUtils;
            this.recordsAccessorFactory = recordsAccessorFactory;

            this.path = fileSystemAccessor.CombinePath(folderPath, FolderName);
            if (!fileSystemAccessor.IsDirectoryExists(this.path))
                fileSystemAccessor.CreateDirectory(this.path);
        }

        public string Store(Stream preloadedDataFile, string fileName)
        {
            var currentFolderId = Guid.NewGuid().FormatGuid();
            var currentFolderPath = this.fileSystemAccessor.CombinePath(this.path, currentFolderId);
            if (this.fileSystemAccessor.IsDirectoryExists(currentFolderPath))
                this.fileSystemAccessor.DeleteDirectory(currentFolderPath);

            this.fileSystemAccessor.CreateDirectory(currentFolderPath);
            using (var fileStream = this.fileSystemAccessor.OpenOrCreateFile(this.fileSystemAccessor.CombinePath(currentFolderPath, this.fileSystemAccessor.GetFileName(fileName)),false))
            {
                preloadedDataFile.CopyTo(fileStream);
            }
            return currentFolderId;
        }

        public PreloadedContentMetaData GetPreloadedDataMetaInformationForSampleData(string id)
        {
            var filesInDirectory = this.GetFiles(id).ToArray();
        
            var csvFilesInDirectory = filesInDirectory.Where(file => this.permittedFileExtensions.Contains(this.fileSystemAccessor.GetFileExtension(file))).ToArray();
            if (csvFilesInDirectory.Length == 0)
            {
                var zipFilesInDirectory = filesInDirectory.Where(this.archiveUtils.IsZipFile).ToArray();
                if (zipFilesInDirectory.Length > 0)
                {
                    return new PreloadedContentMetaData(id,
                        this.fileSystemAccessor.GetFileName(zipFilesInDirectory[0]),
                        this.archiveUtils.GetArchivedFileNamesAndSize(zipFilesInDirectory[0])
                            .Select(
                                file =>
                                    new PreloadedFileMetaData(file.Key, file.Value,
                                        this.permittedFileExtensions.Contains(
                                            this.fileSystemAccessor.GetFileExtension(file.Key))))
                            .ToArray(), PreloadedContentType.Sample);
                }
                return null;
            }

            var csvFile = csvFilesInDirectory[0];
            var csvFileName = this.fileSystemAccessor.GetFileName(csvFile);
            return new PreloadedContentMetaData(id, csvFileName,
                new[] { new PreloadedFileMetaData(csvFileName, this.fileSystemAccessor.GetFileSize(csvFile), true) }, PreloadedContentType.Sample);
        }

        public PreloadedContentMetaData GetPreloadedDataMetaInformationForPanelData(string id)
        {
            var filesInDirectory = this.GetFiles(id);
            var zipFilesInDirectory = filesInDirectory.Where(this.archiveUtils.IsZipFile).ToArray();
            if (zipFilesInDirectory.Length > 0)
            {
                try
                {
                    return new PreloadedContentMetaData(id,
                        this.fileSystemAccessor.GetFileName(zipFilesInDirectory[0]),
                        this.archiveUtils.GetArchivedFileNamesAndSize(zipFilesInDirectory[0])
                            .Select(file => new PreloadedFileMetaData(file.Key, file.Value, this.permittedFileExtensions.Contains(this.fileSystemAccessor.GetFileExtension(file.Key))))
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
            var filesInDirectory = this.GetFiles(id).ToArray();
            var csvFilesInDirectory = filesInDirectory.Where(file => this.permittedFileExtensions.Contains(this.fileSystemAccessor.GetFileExtension(file))).ToArray();
            if (csvFilesInDirectory.Length != 0)
            {
                return this.GetPreloadedDataFromFile(id, csvFilesInDirectory[0]);
            }
            
            var filesInZipArchive= this.TryToGetPreloadedDataFromZipArchive(filesInDirectory, id, this.fileSystemAccessor.CombinePath(this.path, id));
            if (filesInZipArchive.Length==1)
                return filesInZipArchive[0];
            return null;
        }


        public byte[] GetBytesOfSampleData(string id)
        {
            var filesInDirectory = this.GetFiles(id).ToArray();
            var csvFilesInDirectory = filesInDirectory.Where(file => this.permittedFileExtensions.Contains(this.fileSystemAccessor.GetFileExtension(file))).ToArray();
            if (csvFilesInDirectory.Length != 0)
            {
                return this.fileSystemAccessor.ReadAllBytes(csvFilesInDirectory[0]);
            }

            var zipFilesInDirectory = filesInDirectory.Where(this.archiveUtils.IsZipFile).ToArray();
            if (zipFilesInDirectory.Length > 0)
            {
                var archivePath = zipFilesInDirectory[0];
                var currentFolderPath = this.fileSystemAccessor.CombinePath(this.path, id);
                var unzippedDirectoryPath = this.fileSystemAccessor.CombinePath(currentFolderPath, UnzippedFoldername);

                if (!this.fileSystemAccessor.IsDirectoryExists(unzippedDirectoryPath))
                {
                    try
                    {
                        this.archiveUtils.Unzip(archivePath, unzippedDirectoryPath);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e.Message, e);
                        return null;
                    }
                }

                var unzippedFiles =
                    this.fileSystemAccessor.GetFilesInDirectory(unzippedDirectoryPath)
                        .Where(
                            filename =>
                                this.permittedFileExtensions.Contains(this.fileSystemAccessor.GetFileExtension(filename)))
                        .ToArray();

                if (unzippedFiles.Length != 0)
                {
                    return this.fileSystemAccessor.ReadAllBytes(unzippedFiles[0]);
                }
            }
            return null;
        }

        public void DeletePreloadedDataOfSample(string id)
        {
            var currentFolderPath = this.fileSystemAccessor.CombinePath(this.path, id);
            
            if (this.fileSystemAccessor.IsDirectoryExists(currentFolderPath))
                this.fileSystemAccessor.DeleteDirectory(currentFolderPath);
        }

        public PreloadedDataByFile[] GetPreloadedDataOfPanel(string id)
        {
            var currentFolderPath = this.fileSystemAccessor.CombinePath(this.path, id);
            if (!this.fileSystemAccessor.IsDirectoryExists(currentFolderPath))
                return new PreloadedDataByFile[0];

            var filesInDirectory = this.fileSystemAccessor.GetFilesInDirectory(currentFolderPath);
            return this.TryToGetPreloadedDataFromZipArchive(filesInDirectory, id, currentFolderPath);
        }

        public void DeletePreloadedDataOfPanel(string id)
        {
            var currentFolderPath = this.fileSystemAccessor.CombinePath(this.path, id);

            if (this.fileSystemAccessor.IsDirectoryExists(currentFolderPath))
                this.fileSystemAccessor.DeleteDirectory(currentFolderPath);
        }

        private IEnumerable<string> GetFiles(string id)
        {
            var currentFolderPath = this.fileSystemAccessor.CombinePath(this.path, id);
            if (!this.fileSystemAccessor.IsDirectoryExists(currentFolderPath))
                return new string[0];

            return this.fileSystemAccessor.GetFilesInDirectory(currentFolderPath);
        }

        private PreloadedDataByFile[] TryToGetPreloadedDataFromZipArchive(IEnumerable<string> filesInDirectory, string id, string currentFolderPath)
        {
            var archivesInDirectory = filesInDirectory.Where(this.archiveUtils.IsZipFile).ToArray();
            if (archivesInDirectory.Length == 0)
                return new PreloadedDataByFile[0];

            var archivePath = archivesInDirectory[0];
            var unzippedDirectoryPath = this.fileSystemAccessor.CombinePath(currentFolderPath, UnzippedFoldername);

            if (!this.fileSystemAccessor.IsDirectoryExists(unzippedDirectoryPath))
            {
                try
                {
                    this.archiveUtils.Unzip(archivePath, unzippedDirectoryPath);
                }
                catch (Exception e)
                {
                    Logger.Error(e.Message, e);
                    return null;
                }
            }
            var unzippedFiles =
                this.fileSystemAccessor.GetFilesInDirectory(unzippedDirectoryPath).Where(filename => this.permittedFileExtensions.Contains(this.fileSystemAccessor.GetFileExtension(filename))).ToArray();

            if (unzippedFiles.Length == 0)
            {
                var unzippedDirectories = this.fileSystemAccessor.GetDirectoriesInDirectory(unzippedDirectoryPath);
                if (unzippedDirectories == null || unzippedDirectories.Length == 0)
                    return new PreloadedDataByFile[0];

                unzippedFiles = this.fileSystemAccessor.GetFilesInDirectory(unzippedDirectories[0]);
            }

            return unzippedFiles
                .Where(filename => this.permittedFileExtensions.Contains(this.fileSystemAccessor.GetFileExtension(filename)))
                .Select(file => this.GetPreloadedDataFromFile(id, file))
                .Where(data => data != null)
                .ToArray();
        }

        private PreloadedDataByFile GetPreloadedDataFromFile(string id, string fileInDirectory)
        {
            string[] header = null;
            var records = new List<string[]>();
            try
            {
                using (var fileStream = this.fileSystemAccessor.ReadFile(fileInDirectory))
                {
                    var recordAccessor = this.recordsAccessorFactory.CreateRecordsAccessor(fileStream, ExportFileSettings.DataFileSeparator.ToString());

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
                Logger.Error($"Error on getting data from file {fileInDirectory}. " + e.Message, e);
                return null;
            }

            return new PreloadedDataByFile(id, this.fileSystemAccessor.GetFileName(fileInDirectory), header?? new string[0], records.ToArray());
        }
    }
}
