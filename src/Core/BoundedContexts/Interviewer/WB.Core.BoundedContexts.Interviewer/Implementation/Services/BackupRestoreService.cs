using System;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class BackupRestoreService : IBackupRestoreService
    {
        private readonly IArchiveUtils archiver;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IInterviewerSettings interviewerSettings;
        private readonly string privateStorage;

        public BackupRestoreService(
            IArchiveUtils archiver,
            IFileSystemAccessor fileSystemAccessor, 
            IInterviewerSettings interviewerSettings, 
            string privateStorage)
        {
            this.archiver = archiver;
            this.fileSystemAccessor = fileSystemAccessor;
            this.interviewerSettings = interviewerSettings;
            this.privateStorage = privateStorage;
        }

        public Task<byte[]> GetSystemBackupAsync()
        {
            return Task.Run(() =>
            {
                var pathToCrushFile = this.interviewerSettings.CrushFilePath;

                if (this.fileSystemAccessor.IsFileExists(pathToCrushFile))
                    this.fileSystemAccessor.CopyFileOrDirectory(pathToCrushFile, privateStorage);

                return this.archiver.ZipDirectoryToByteArray(privateStorage,
                    fileFilter: @"\.log$;\.dll$;\.mdb$;");
            });
        }

        public async Task<string> BackupAsync(string backupToFolderPath)
        {
            var backupFileName = $"backup-interviewer-{DateTime.Now.ToString("yyyyMMddTH-mm-ss")}.ibak";
            var backup = await this.GetSystemBackupAsync();
            var emptyBackupFile =
                this.fileSystemAccessor.CombinePath(backupToFolderPath, backupFileName);
            await Task.Run(() =>
            {
                if (!this.fileSystemAccessor.IsDirectoryExists(backupToFolderPath))
                    this.fileSystemAccessor.CreateDirectory(backupToFolderPath);

                this.fileSystemAccessor.WriteAllBytes(emptyBackupFile, backup);
            });
            return emptyBackupFile;
        }

        public async Task RestoreAsync(string backupFilePath)
        {
            if (this.fileSystemAccessor.IsFileExists(backupFilePath))
            {
                await Task.Run(() => this.archiver.Unzip(backupFilePath, privateStorage, true));
            }
        }
    }
}