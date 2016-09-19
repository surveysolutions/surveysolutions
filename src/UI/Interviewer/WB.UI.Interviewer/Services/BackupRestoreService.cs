using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SQLite;
using SQLitePCL;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;

namespace WB.UI.Interviewer.Services
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
            return await this.BackupAsync(this.privateStorage);
        }

        public async Task<string> BackupAsync(string backupToFolderPath)
        {
            var isBackupFolderExists = await this.fileSystemAccessor.IsDirectoryExistsAsync(backupToFolderPath);
            if (!isBackupFolderExists)
                await this.fileSystemAccessor.CreateDirectoryAsync(backupToFolderPath);

            var isLogFolderExists = await this.fileSystemAccessor.IsDirectoryExistsAsync(this.logDirectoryPath);
            if (isLogFolderExists)
                await this.fileSystemAccessor.CopyDirectoryAsync(this.logDirectoryPath, this.privateStorage);

            this.BackupSqliteDbs();

            var backupFileName = $"backup-interviewer-{DateTime.Now:s}.ibak";
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
            foreach (var dbPathToBackup in Directory.GetFiles(this.privateStorage, "*.sqlite3", SearchOption.AllDirectories))
            {
                string destDBPath;
                using (var connection = new SQLiteConnectionWithLock(dbPathToBackup, openFlags: SQLiteOpenFlags.ReadOnly | SQLiteOpenFlags.FullMutex))
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
        }

        private void CreateDatabaseBackup(SQLiteConnectionWithLock connection, string destDBPath)
        {
            sqlite3 destDB;
            SQLite3.Result r = (SQLite3.Result) 
                raw.sqlite3_open_v2(destDBPath, out destDB, (int)(SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite), null);
            
            if (r != SQLite3.Result.OK)
            {
                throw SQLiteException.New(r, $"Could not open backup database file: {destDBPath} (raw sqlite result: {r})");
            }

            using (connection.Lock())
            {
                /* Open the backup object used to accomplish the transfer */
                var bHandle = raw.sqlite3_backup_init(destDB, "main", connection.Handle, "main");

                if (bHandle == null)
                {
                    // Close the database connection 
                    raw.sqlite3_close_v2(destDB);

                    throw SQLiteException.New(r, $"Could not initiate backup process: {destDBPath}");
                }

                /* Each iteration of this loop copies 5 database pages from database
                ** pDb to the backup database. If the return value of backup_step()
                ** indicates that there are still further pages to copy, sleep for
                ** 250 ms before repeating. */
                do
                {
                    r = (SQLite3.Result) raw.sqlite3_backup_step(bHandle, 5);

                    if (r == SQLite3.Result.OK || r == SQLite3.Result.Busy || r == SQLite3.Result.Locked)
                    {
                        Thread.Sleep(250);
                    }
                } while (r == SQLite3.Result.OK || r == SQLite3.Result.Busy || r == SQLite3.Result.Locked);

                /* Release resources allocated by backup_init(). */
                r = (SQLite3.Result) raw.sqlite3_backup_finish(bHandle);

                if (r != SQLite3.Result.OK)
                {
                    // Close the database connection 
                    raw.sqlite3_close_v2(destDB);

                    throw SQLiteException.New(r, $"Could not finish backup process: {destDBPath} (sqlite result: {r})");
                }

                // Close the database connection 
                raw.sqlite3_close_v2(destDB);
            }
        }

        private static byte[] GetNullTerminatedUtf8(string s)
        {
            var utf8Length = Encoding.UTF8.GetByteCount(s);
            var bytes = new byte[utf8Length + 1];
            Encoding.UTF8.GetBytes(s, 0, s.Length, bytes, 0);
            return bytes;
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