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

        public TroubleshootingService(IArchiveUtils archiver)
        {
            this.archiver = archiver;
        }

        public async Task<byte[]> GetSystemBackupAsync()
        {
            return await Task.FromResult(
                this.archiver.ZipDirectoryToByteArray(FileSystem.Current.LocalStorage.Path, fileFilter: @"\.log$;\.dll$;\.mdb$;"));
        }

        public async Task BackupAsync(string backupToFolderPath)
        {
            var backupFileName = $"backup-interviewer-{DateTime.Now.ToString("yyyyMMddTH-mm")}.ibak";
            var backupFolder = await FileSystem.Current.GetFolderFromPathAsync(backupToFolderPath);
            var emptyBackupFile = await backupFolder.CreateFileAsync(backupFileName, CreationCollisionOption.GenerateUniqueName);

            var backup = await this.GetSystemBackupAsync();
            using (var stream = await emptyBackupFile.OpenAsync(FileAccess.ReadAndWrite))
            {
                stream.Write(backup, 0, backup.Length);
            }
        }

        public async Task RestoreAsync(string backupFilePath)
        {
            await Task.Run(() => this.archiver.Unzip(backupFilePath, FileSystem.Current.LocalStorage.Path, true));
        }
    }
}