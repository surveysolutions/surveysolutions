using System;
using System.Threading.Tasks;
using PCLStorage;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class BackupRestoreService : IBackupRestoreService
    {
        private readonly IArchiveUtils archiver;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IInterviewerSettings interviewerSettings;

        public BackupRestoreService(IArchiveUtils archiver,
            IFileSystemAccessor fileSystemAccessor, IInterviewerSettings interviewerSettings)
        {
            this.archiver = archiver;
            this.fileSystemAccessor = fileSystemAccessor;
            this.interviewerSettings = interviewerSettings;
        }

        public Task<byte[]> GetSystemBackupAsync()
        {
            return Task.Run(() =>
            {
                var filesInCrashFolder =
                    this.fileSystemAccessor.GetFilesInDirectory(this.interviewerSettings.CrushFolder);
                var crashDirectory = this.fileSystemAccessor.CombinePath(FileSystem.Current.LocalStorage.Path,
                    "crashes");
                if(!this.fileSystemAccessor.IsDirectoryExists(crashDirectory))
                    this.fileSystemAccessor.CreateDirectory(crashDirectory);

                foreach (var fileInCrushFolder in filesInCrashFolder)
                {
                    this.fileSystemAccessor.CopyFileOrDirectory(fileInCrushFolder, crashDirectory);
                }

                return this.archiver.ZipDirectoryToByteArray(FileSystem.Current.LocalStorage.Path,
                    fileFilter: @"\.log$;\.dll$;\.mdb$;");
            });
        }

        public async Task<string> BackupAsync(string backupToFolderPath)
        {
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