using System;
using System.Threading.Tasks;
using PCLStorage;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class TroubleshootingService : ITroubleshootingService
    {
        private readonly IArchiveUtils archiver;
        private readonly IFileSystemAccessor fileSystemAccessor;

        public TroubleshootingService(IArchiveUtils archiver,
            IFileSystemAccessor fileSystemAccessor)
        {
            this.archiver = archiver;
            this.fileSystemAccessor = fileSystemAccessor;
        }

        public Task<byte[]> GetSystemBackupAsync()
        {
            return Task.Run(() =>

                this.archiver.ZipDirectoryToByteArray(FileSystem.Current.LocalStorage.Path,
                    fileFilter: @"\.log$;\.dll$;\.mdb$;"));
        }

        public async Task<string> BackupAsync(string backupToFolderPath)
        {
            if (!this.fileSystemAccessor.IsDirectoryExists(backupToFolderPath))
            {
                this.fileSystemAccessor.CreateDirectory(backupToFolderPath);
            }

            IFolder backupToFolder = await FileSystem.Current.GetFolderFromPathAsync(backupToFolderPath);
            var backupFileName = $"backup-interviewer-{DateTime.Now.ToString("yyyyMMddTH-mm-ss")}.ibak";
            var emptyBackupFile = await backupToFolder.CreateFileAsync(backupFileName, CreationCollisionOption.GenerateUniqueName);
            var backup = await this.GetSystemBackupAsync();
            using (var stream = await emptyBackupFile.OpenAsync(FileAccess.ReadAndWrite))
            {
                stream.Write(backup, 0, backup.Length);
            }
            return emptyBackupFile.Path;
        }

        public Task RestoreAsync(string backupFilePath)
        {
            return Task.Run(() =>
            {
                if (this.fileSystemAccessor.IsFileExists(backupFilePath))
                {
                    this.archiver.Unzip(backupFilePath, FileSystem.Current.LocalStorage.Path, true);
                }
            });
        }
    }
}