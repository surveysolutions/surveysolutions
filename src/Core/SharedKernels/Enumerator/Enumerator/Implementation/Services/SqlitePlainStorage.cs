using System.Reflection;
using SQLite;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Workspace;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class SqlitePlainStorage<TEntity> : SqlitePlainStorage<TEntity, string>, IPlainStorage<TEntity>
        where TEntity : class, IPlainStorageEntity, new()
    {
        public SqlitePlainStorage(ILogger logger, IFileSystemAccessor fileSystemAccessor, SqliteSettings settings,
            IWorkspaceAccessor workspaceAccessor) 
            : base(logger, fileSystemAccessor, settings, workspaceAccessor)
        {
        }

        public SqlitePlainStorage(SQLiteConnectionWithLock storage, ILogger logger) : base(storage, logger)
        {
        }
    }

    public class SqlitePlainStorage<TEntity, TKey> : SqlitePlainStorageWithoutWorkspace<TEntity, TKey>
        where TEntity : class, IPlainStorageEntity<TKey>, new()
    {
        private readonly IWorkspaceAccessor workspaceAccessor;

        public SqlitePlainStorage(ILogger logger,
            IFileSystemAccessor fileSystemAccessor,
            SqliteSettings settings,
            IWorkspaceAccessor workspaceAccessor)
        : base(logger, fileSystemAccessor, settings)
        {
            this.workspaceAccessor = workspaceAccessor;
        }

        protected override string GetPathToDatabase()
        {
            var entityName = typeof(TEntity).Name;
            var pathToDatabase = settings.PathToDatabaseDirectory;
            
            var workspaces = typeof(TEntity).GetCustomAttribute(typeof(WorkspacesAttribute));
            if (workspaces == null)
            {
                var workspaceName = workspaceAccessor.GetCurrent()?.Name;
                if (!string.IsNullOrEmpty(workspaceName))
                    pathToDatabase = fileSystemAccessor.CombinePath(pathToDatabase, workspaceName);
            }

            pathToDatabase = fileSystemAccessor.CombinePath(pathToDatabase, entityName + "-data.sqlite3");
            return pathToDatabase;
        }
        
        public SqlitePlainStorage(SQLiteConnectionWithLock storage, ILogger logger)
            :base(storage, logger)
        {
            
        }
    }
}
