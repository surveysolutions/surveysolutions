using System.Linq;
using Cirrious.MvvmCross.Plugins.Sqlite;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Backup;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.UI.Interviewer.Syncronization.Implementation
{
    public class InterviewPackageIdsStorage : IInterviewPackageIdsStorage, IBackupable
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IPrincipal principal;
        private readonly ISQLiteConnectionFactory connectionFactory;
        private readonly InterviewPackageIdsStorageSettings settings;

        private string fullPathToDataBase
        {
            get { return this.fileSystemAccessor.CombinePath(this.settings.PathToDatabase, this.settings.DatabaseName); }
        }

        public InterviewPackageIdsStorage(IFileSystemAccessor fileSystemAccessor, IPrincipal principal,
            ISQLiteConnectionFactory connectionFactory, InterviewPackageIdsStorageSettings settings)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.principal = principal;
            this.connectionFactory = connectionFactory;
            this.settings = settings;
            using (var connection = this.connectionFactory.Create(this.fullPathToDataBase))
            {
                connection.CreateTable<SyncPackageId>();
            }
        }

        public void Store(string packageId, long sortIndex)
        {
            var userIdAsString = this.principal.CurrentUserIdentity.UserId.FormatGuid();
            using (var connection = this.connectionFactory.Create(this.fullPathToDataBase))
            {
                var newId = new SyncPackageId
                {
                    PackageId = packageId,
                    SortIndex = sortIndex,
                    UserId = userIdAsString,
                    Type = SyncItemType.Interview
                };

                connection.Insert(newId);
            }
        }

        public string GetLastStoredPackageId()
        {
            var userIdAsString = this.principal.CurrentUserIdentity.UserId.FormatGuid();
            using (var connection = this.connectionFactory.Create(this.fullPathToDataBase))
            {
                var lastStoredChunkId = connection.Table<SyncPackageId>()
                    .Where(x => x.Type == SyncItemType.Interview && x.UserId == userIdAsString)
                    .OrderBy(x => x.SortIndex)
                    .LastOrDefault();

                return lastStoredChunkId == null ? null : lastStoredChunkId.PackageId;
            }
        }

        public string GetPathToBackupFile()
        {
            return this.fullPathToDataBase;
        }

        public void RestoreFromBackupFolder(string path)
        {
            var sourceFile = this.fileSystemAccessor.CombinePath(path, this.settings.DatabaseName);

            this.fileSystemAccessor.DeleteFile(this.fullPathToDataBase);
            this.fileSystemAccessor.CopyFileOrDirectory(sourceFile, this.settings.PathToDatabase);
        }
    }
}