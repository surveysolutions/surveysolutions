﻿using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Infrastructure.Security;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    public class DataExportFileAccessor : IDataExportFileAccessor
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IExportSettings exportSettings;
        private readonly IPlainTransactionManagerProvider plainTransactionManagerProvider;
        private readonly IProtectedArchiveUtils archiveUtils;
        private readonly ILogger logger;

        private IPlainTransactionManager PlainTransactionManager => this.plainTransactionManagerProvider.GetPlainTransactionManager();

        public DataExportFileAccessor(IFileSystemAccessor fileSystemAccessor, 
            IExportSettings exportSettings, 
            IPlainTransactionManagerProvider plainTransactionManagerProvider, 
            IProtectedArchiveUtils archiveUtils,
            ILogger logger)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.exportSettings = exportSettings;
            this.plainTransactionManagerProvider = plainTransactionManagerProvider;
            this.archiveUtils = archiveUtils;
            this.logger = logger;
        }

        public void RecreateExportArchive(string folderForDataExport, string archiveFilePath)
        {
            if (this.fileSystemAccessor.IsFileExists(archiveFilePath))
            {
                this.fileSystemAccessor.DeleteFile(archiveFilePath);
            }

            var password = this.GetPasswordFromSettings();
            this.archiveUtils.ZipDirectory(folderForDataExport, archiveFilePath, password);
        }

        public void RecreateExportArchive(IEnumerable<string> filesToArchive, string archiveFilePath)
        {
            if (this.fileSystemAccessor.IsFileExists(archiveFilePath))
            {
                this.logger.Info($"{archiveFilePath} existed, deleting");
                this.fileSystemAccessor.DeleteFile(archiveFilePath);
            }

            this.logger.Debug($"Starting creation of {Path.GetFileName(archiveFilePath)} archive");
            Stopwatch watch = Stopwatch.StartNew();

            var password = this.GetPasswordFromSettings();
            this.archiveUtils.ZipFiles(filesToArchive, archiveFilePath, password);

            watch.Stop();
            this.logger.Info($"Done creation {Path.GetFileName(archiveFilePath)} archive. Spent: {watch.Elapsed:g}");
        }

        private string GetPasswordFromSettings()
        {
            return this.PlainTransactionManager.ExecuteInPlainTransaction(() =>
                this.exportSettings.EncryptionEnforced()
                    ? this.exportSettings.GetPassword()
                    : null);
        }
    }
}