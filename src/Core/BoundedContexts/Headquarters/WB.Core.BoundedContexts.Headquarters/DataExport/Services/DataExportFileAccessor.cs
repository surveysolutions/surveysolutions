﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Ionic.Zlib;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.Storage;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    public class DataExportFileAccessor : IDataExportFileAccessor
    {
        private readonly IExportSettings exportSettings;
        private readonly IPlainTransactionManagerProvider plainTransactionManagerProvider;
        private readonly IProtectedArchiveUtils archiveUtils;
        private readonly IExternalFileStorage externalFileStorage;
        private readonly ILogger logger;

        private IPlainTransactionManager PlainTransactionManager => this.plainTransactionManagerProvider.GetPlainTransactionManager();

        public DataExportFileAccessor(IExportSettings exportSettings, 
            IPlainTransactionManagerProvider plainTransactionManagerProvider, 
            IProtectedArchiveUtils archiveUtils,
            ILogger logger, 
            IExternalFileStorage externalFileStorage)
        {
            this.exportSettings = exportSettings;
            this.plainTransactionManagerProvider = plainTransactionManagerProvider;
            this.archiveUtils = archiveUtils;
            this.logger = logger;
            this.externalFileStorage = externalFileStorage;
        }

        public void RecreateExportArchive(string folderForDataExport, string archiveFilePath, IProgress<int> progress = null)
        {
            var password = this.GetPasswordFromSettings();
            this.archiveUtils.ZipDirectory(folderForDataExport, archiveFilePath, password, progress);
        }

        public void RecreateExportArchive(IEnumerable<string> filesToArchive, string archiveFilePath)
        {
            this.logger.Debug($"Starting creation of {Path.GetFileName(archiveFilePath)} archive");
            Stopwatch watch = Stopwatch.StartNew();

            var password = this.GetPasswordFromSettings();
            this.archiveUtils.ZipFiles(filesToArchive, archiveFilePath, password);

            watch.Stop();
            this.logger.Info($"Done creation {Path.GetFileName(archiveFilePath)} archive. Spent: {watch.Elapsed:g}");
        }

        public IZipArchive CreateExportArchive(Stream outputStream, CompressionLevel compressionLevel = CompressionLevel.BestSpeed)
        {
            var password = this.GetPasswordFromSettings();
            return new IonicZipArchive(outputStream, password, compressionLevel);
        }

        public void PubishArchiveToExternalStorage(string archiveFile, IProgress<int> exportProgress)
        {
            if (externalFileStorage.IsEnabled())
            {
                using (var file = File.OpenRead(archiveFile))
                {
                    var name = Path.GetFileName(archiveFile);
                    this.externalFileStorage.Store($"export/{name}", file, "application/zip", exportProgress);
                }
            }
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