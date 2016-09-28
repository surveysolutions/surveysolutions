using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SQLite;
using SQLitePCL;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;

namespace WB.UI.Interviewer.Services
{
    public class BackupRestoreService : IBackupRestoreService
    {
        private readonly IArchiveUtils archiver;
        private readonly IAsynchronousFileSystemAccessor fileSystemAccessor;
        private readonly ILogger logger;
        private readonly string privateStorage;
        private readonly string logDirectoryPath;

        public BackupRestoreService(
            IArchiveUtils archiver,
            IAsynchronousFileSystemAccessor fileSystemAccessor,
            ILogger logger,
            string privateStorage,
            string logDirectoryPath)
        {
            this.archiver = archiver;
            this.fileSystemAccessor = fileSystemAccessor;
            this.logger = logger;
            this.privateStorage = privateStorage;
            this.logDirectoryPath = logDirectoryPath;
        }

        public async Task<string> BackupAsync()
        {
            return await this.BackupAsync(this.privateStorage).ConfigureAwait(false);
        }

        public async Task<string> BackupAsync(string backupToFolderPath)
        {
            var isBackupFolderExists = await this.fileSystemAccessor.IsDirectoryExistsAsync(backupToFolderPath).ConfigureAwait(false);
            if (!isBackupFolderExists)
                await this.fileSystemAccessor.CreateDirectoryAsync(backupToFolderPath).ConfigureAwait(false);

            this.BackupSqliteDbs();

            var isLogFolderExists = await this.fileSystemAccessor.IsDirectoryExistsAsync(this.logDirectoryPath).ConfigureAwait(false);
            if (isLogFolderExists)
                await this.fileSystemAccessor.CopyDirectoryAsync(this.logDirectoryPath, this.privateStorage)
                                             .ConfigureAwait(false);

            var backupFileName = $"backup-interviewer-{DateTime.Now:s}.ibak";
            var backupFilePath = this.fileSystemAccessor.CombinePath(backupToFolderPath, backupFileName);

            await this.archiver.ZipDirectoryToFileAsync(this.privateStorage, backupFilePath, fileFilter: @"\.log$;\.dll$;\.sqlite3.back$;")
                               .ConfigureAwait(false);

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
            foreach (var dbPathToBackup in Directory.GetFiles(this.privateStorage, "*.sqlite3", SearchOption.AllDirectories))
            {
                try
                {
                    string destDBPath;
                    using (var connection = new SQLiteConnectionWithLock(dbPathToBackup, openFlags: SQLiteOpenFlags.ReadOnly))
                    {
                        destDBPath = $"{connection.DatabasePath}.{DateTime.UtcNow:yyyy-MM-dd_HH-mm-ss-fff}";

                        CreateDatabaseBackup(connection, destDBPath);
                    }

                    var destFileName = Path.ChangeExtension(destDBPath, "back");
                    if (File.Exists(destFileName))
                    {
                        File.Delete(destFileName);
                    }

                    File.Move(destDBPath, destFileName);
                }
                catch (Exception ex)
                {
                    this.logger.Error(ex.Message, ex);
                    File.Copy(dbPathToBackup, $"{dbPathToBackup}.back");
                }
            }
        }

        private void CreateDatabaseBackup(SQLiteConnectionWithLock connection, string destDBPath)
        {
            sqlite3 destDB;
            SQLite3.Result destKbResult = (SQLite3.Result)
                raw.sqlite3_open_v2(destDBPath, out destDB, (int) (SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite),
                    null);

            if (destKbResult != SQLite3.Result.OK)
            {
                throw SQLiteException.New(destKbResult,
                    $"Could not open backup database file: {destDBPath} (raw sqlite result: {destKbResult})");
            }
            /* Open the backup object used to accomplish the transfer */
            var bHandle = raw.sqlite3_backup_init(destDB, "main", connection.Handle, "main");

            if (bHandle == null)
            {
                // Close the database connection 
                raw.sqlite3_close_v2(destDB);

                throw SQLiteException.New(destKbResult, $"Could not initiate backup process: {destDBPath}");
            }

            /* Each iteration of this loop copies 5 database pages from database
            ** pDb to the backup database. If the return value of backup_step()
            ** indicates that there are still further pages to copy, sleep for
            ** 250 ms before repeating. */
            do
            {
                destKbResult = (SQLite3.Result) raw.sqlite3_backup_step(bHandle, 30);

                if (destKbResult == SQLite3.Result.OK || destKbResult == SQLite3.Result.Busy ||
                    destKbResult == SQLite3.Result.Locked)
                {
                    Thread.Sleep(250);
                }
            } while (destKbResult == SQLite3.Result.OK || destKbResult == SQLite3.Result.Busy ||
                     destKbResult == SQLite3.Result.Locked);

            /* Release resources allocated by backup_init(). */
            destKbResult = (SQLite3.Result) raw.sqlite3_backup_finish(bHandle);

            if (destKbResult != SQLite3.Result.OK)
            {
                // Close the database connection 
                raw.sqlite3_close_v2(destDB);

                throw SQLiteException.New(destKbResult,
                    $"Could not finish backup process: {destDBPath} (sqlite result: {destKbResult})");
            }

            // Close the database connection 
            raw.sqlite3_close_v2(destDB);
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