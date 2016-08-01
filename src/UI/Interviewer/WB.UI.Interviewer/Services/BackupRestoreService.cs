using System;
using System.IO;
using System.Threading.Tasks;
using SQLite.Net;
using SQLite.Net.Interop;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.Infrastructure.FileSystem;

namespace WB.UI.Interviewer.Services
{
    public class BackupRestoreService : IBackupRestoreService
    {
        private readonly IArchiveUtils archiver;
        private readonly IAsynchronousFileSystemAccessor fileSystemAccessor;
        private readonly ISQLitePlatform sqLitePlatform;
        private readonly string privateStorage;
        private readonly string logDirectoryPath;

        public BackupRestoreService(
            IArchiveUtils archiver,
            IAsynchronousFileSystemAccessor fileSystemAccessor,
            ISQLitePlatform sqLitePlatform,
            string privateStorage,
            string logDirectoryPath)
        {
            this.archiver = archiver;
            this.fileSystemAccessor = fileSystemAccessor;
            this.sqLitePlatform = sqLitePlatform;
            this.privateStorage = privateStorage;
            this.logDirectoryPath = logDirectoryPath;
        }

        public async Task<string> BackupAsync()
        {
            return await this.BackupAsync(this.privateStorage);
        }

        public async Task<string> BackupAsync(string backupToFolderPath)
        {
            var isBackupFolderExists = await this.fileSystemAccessor.IsDirectoryExistsAsync(backupToFolderPath);
            if (!isBackupFolderExists)
                await this.fileSystemAccessor.CreateDirectoryAsync(backupToFolderPath);

            if (await this.fileSystemAccessor.IsDirectoryExistsAsync(this.logDirectoryPath))
                await this.fileSystemAccessor.CopyDirectoryAsync(this.logDirectoryPath, this.privateStorage);

            this.BackupSqliteDbs();

            var backupFileName = $"backup-interviewer-{DateTime.Now.ToString("s")}.ibak";
            var backupFilePath = this.fileSystemAccessor.CombinePath(backupToFolderPath, backupFileName);

            await this.archiver.ZipDirectoryToFileAsync(this.privateStorage, backupFilePath, fileFilter: @"\.log$;\.dll$;\.sqlite3.back$;");

            this.Cleanup();

            return backupFilePath;
        }

        private void Cleanup()
        {
            foreach (var tempBackupFile in Directory.GetFiles(this.privateStorage, "*.sqlite3.back", SearchOption.AllDirectories))
            {
                File.Delete(tempBackupFile);
            }
        }

        private void BackupSqliteDbs()
        {
            foreach (var sqliteDbPath in Directory.GetFiles(this.privateStorage, "*.sqlite3", SearchOption.AllDirectories))
            {
                string slite3BackupPath;
                using (var connection = new SQLiteConnectionWithLock(this.sqLitePlatform,
                    new SQLiteConnectionString(sqliteDbPath, 
                        true,
                        openFlags: SQLiteOpenFlags.ReadOnly | SQLiteOpenFlags.FullMutex)))
                {
                    slite3BackupPath = connection.CreateDatabaseBackup(this.sqLitePlatform);
                }

                var destFileName = Path.ChangeExtension(slite3BackupPath, "back");
                if (File.Exists(destFileName))
                {
                    File.Delete(destFileName);
                }

                File.Move(slite3BackupPath, destFileName);
            }
        }

        public async Task RestoreAsync(string backupFilePath)
        {
            if (await this.fileSystemAccessor.IsFileExistsAsync(backupFilePath))
            {
                if (await this.fileSystemAccessor.IsDirectoryExistsAsync(this.privateStorage))
                {
                    await this.fileSystemAccessor.RemoveDirectoryAsync(this.privateStorage);
                    await this.fileSystemAccessor.CreateDirectoryAsync(this.privateStorage);
                }

                await this.archiver.UnzipAsync(backupFilePath, this.privateStorage, true);

                foreach (var tempBackupFile in Directory.GetFiles(this.privateStorage, "*.sqlite3.back", SearchOption.AllDirectories))
                {
                    var destFileName = Path.ChangeExtension(tempBackupFile, null);
                    File.Move(tempBackupFile, destFileName);
                }
            }
        }
    }
}