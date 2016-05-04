using System;
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
        private readonly string logFilePath;

        public BackupRestoreService(
            IArchiveUtils archiver,
            IAsynchronousFileSystemAccessor fileSystemAccessor, 
            string privateStorage,
            string logFilePath)
        {
            this.archiver = archiver;
            this.fileSystemAccessor = fileSystemAccessor;
            this.privateStorage = privateStorage;
            this.logFilePath = logFilePath;
        }

        public async Task<byte[]> GetSystemBackupAsync()
        {
            if (await this.fileSystemAccessor.IsFileExistsAsync(this.logFilePath))
                await this.fileSystemAccessor.CopyFileAsync(this.logFilePath, privateStorage);

            return await this.archiver.ZipDirectoryToByteArrayAsync(privateStorage,
                fileFilter: @"\.log$;\.dll$;\.sqlite3$;");
        }

        public async Task<string> BackupAsync(string backupToFolderPath)
        {
            var backupFileName = $"backup-interviewer-{DateTime.Now.ToString("yyyyMMddTH-mm-ss")}.ibak";
            var backup = await this.GetSystemBackupAsync();
            var backupFilePath = this.fileSystemAccessor.CombinePath(backupToFolderPath, backupFileName);

            var isBackupFolderExists = await this.fileSystemAccessor.IsDirectoryExistsAsync(backupToFolderPath);
            if (!isBackupFolderExists)
                await this.fileSystemAccessor.CreateDirectoryAsync(backupToFolderPath);

            await this.fileSystemAccessor.WriteAllBytesAsync(backupFilePath, backup);
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