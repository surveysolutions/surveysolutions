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
        private readonly IInterviewerSettings interviewerSettings;
        private readonly string privateStorage;

        public BackupRestoreService(
            IArchiveUtils archiver,
            IAsynchronousFileSystemAccessor fileSystemAccessor, 
            IInterviewerSettings interviewerSettings, 
            string privateStorage)
        {
            this.archiver = archiver;
            this.fileSystemAccessor = fileSystemAccessor;
            this.interviewerSettings = interviewerSettings;
            this.privateStorage = privateStorage;
        }

        public async Task<byte[]> GetSystemBackupAsync()
        {
            var pathToCrushFile = this.interviewerSettings.CrushFilePath;

            if (await this.fileSystemAccessor.IsFileExistsAsync(pathToCrushFile))
                await this.fileSystemAccessor.CopyFileAsync(pathToCrushFile, privateStorage);

            return await this.archiver.ZipDirectoryToByteArrayAsync(privateStorage,
                fileFilter: @"\.log$;\.dll$;\.mdb$;");
        }

        public async Task<string> BackupAsync(string backupToFolderPath)
        {
            var backupFileName = $"backup-interviewer-{DateTime.Now.ToString("yyyyMMddTH-mm-ss")}.ibak";
            var backup = await this.GetSystemBackupAsync();
            var emptyBackupFile = this.fileSystemAccessor.CombinePath(backupToFolderPath, backupFileName);

            var isBackupFolderExists = await this.fileSystemAccessor.IsDirectoryExistsAsync(backupToFolderPath);
            if (!isBackupFolderExists)
                await this.fileSystemAccessor.CreateDirectoryAsync(backupToFolderPath);

            await this.fileSystemAccessor.WriteAllBytesAsync(emptyBackupFile, backup);
            return emptyBackupFile;
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