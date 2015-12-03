using System.IO;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class TroubleshootingService : ITroubleshootingService
    {
        private readonly IArchiveUtils archiver;
        private readonly IFileSystemAccessor fileSystemAccessor;

        public TroubleshootingService(IArchiveUtils archiver, IFileSystemAccessor fileSystemAccessor)
        {
            this.archiver = archiver;
            this.fileSystemAccessor = fileSystemAccessor;
        }

        public async Task<byte[]> GetSystemBackupAsync()
        {
            return await Task.FromResult(
                this.archiver.ZipDirectoryToByteArray(PCLStorage.FileSystem.Current.LocalStorage.Path, fileFilter: @"\.log$;\.dll$;\.mdb$;"));
        }

        public async Task BackupAsync(string backupFilePath)
        {
            var backup = await this.GetSystemBackupAsync();
            var backupFile = this.fileSystemAccessor.OpenOrCreateFile(backupFilePath, false);
            await new MemoryStream(backup).CopyToAsync(backupFile);
        }

        public async Task RestoreAsync(string backupFilePath)
        {
            await Task.Run(() => this.archiver.Unzip(backupFilePath, PCLStorage.FileSystem.Current.LocalStorage.Path));
        }
    }
}