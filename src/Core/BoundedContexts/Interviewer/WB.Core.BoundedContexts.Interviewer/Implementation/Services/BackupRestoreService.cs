using System;
using System.IO;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class BackupRestoreService : IBackupRestoreService
    {
        private readonly IArchiveUtils archiver;
        private readonly IAsynchronousFileSystemAccessor fileSystemAccessor;
        private readonly string privateStorage;
        private readonly string logDirectoryPath;

        public BackupRestoreService(
            IArchiveUtils archiver,
            IAsynchronousFileSystemAccessor fileSystemAccessor, 
            string privateStorage,
            string logDirectoryPath)
        {
            this.archiver = archiver;
            this.fileSystemAccessor = fileSystemAccessor;
            this.privateStorage = privateStorage;
            this.logDirectoryPath = logDirectoryPath;
        }

        public async Task<string> BackupAsync()
        {
            return await BackupAsync(this.privateStorage);
        }

        public async Task<string> BackupAsync(string backupToFolderPath)
        {
            var isBackupFolderExists = await this.fileSystemAccessor.IsDirectoryExistsAsync(backupToFolderPath);
            if (!isBackupFolderExists)
                await this.fileSystemAccessor.CreateDirectoryAsync(backupToFolderPath);

            if (await this.fileSystemAccessor.IsDirectoryExistsAsync(this.logDirectoryPath))
                await this.fileSystemAccessor.CopyDirectoryAsync(this.logDirectoryPath, privateStorage);

            var backupFileName = $"backup-interviewer-{DateTime.Now.ToString("yyyyMMddTH-mm-ss")}.ibak";
            var backupFilePath = this.fileSystemAccessor.CombinePath(backupToFolderPath, backupFileName);

            await this.archiver.ZipDirectoryToFileAsync(privateStorage, backupFilePath, fileFilter: @"\.log$;\.dll$;\.sqlite3$;");

            return backupFilePath;
        }

        public async Task RestoreAsync(string backupFilePath)
        {
            if (await this.fileSystemAccessor.IsFileExistsAsync(backupFilePath))
            {
                await this.archiver.UnzipAsync(backupFilePath, privateStorage, true);
            }
        }
    }
}