using System.Reflection;
using SQLite;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Workspace;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class SqlitePlainStorageWithWorkspace<TEntity> : SqlitePlainStorageWithWorkspace<TEntity, string>, IPlainStorage<TEntity>
        where TEntity : class, IPlainStorageEntity, new()
    {
        public SqlitePlainStorageWithWorkspace(ILogger logger, IFileSystemAccessor fileSystemAccessor, SqliteSettings settings,
            IWorkspaceAccessor workspaceAccessor) 
            : base(logger, fileSystemAccessor, settings, workspaceAccessor)
        {
        }

        public SqlitePlainStorageWithWorkspace(SQLiteConnectionWithLock storage, ILogger logger) : base(storage, logger)
        {
        }
    }

    public class SqlitePlainStorageWithWorkspace<TEntity, TKey> : SqlitePlainStorage<TEntity, TKey>
        where TEntity : class, IPlainStorageEntity<TKey>, new()
    {
        private readonly IWorkspaceAccessor workspaceAccessor;

        public SqlitePlainStorageWithWorkspace(ILogger logger,
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
            string pathToDatabase = settings.PathToDatabaseDirectory;
            
            var workspaces = typeof(TEntity).GetCustomAttribute(typeof(NonWorkspacedAttribute));
            if (workspaces == null)
            {
                var workspaceName = workspaceAccessor.GetCurrentWorkspaceName();
                if (!string.IsNullOrEmpty(workspaceName))
                    pathToDatabase = fileSystemAccessor.CombinePath(settings.PathToRootDirectory, workspaceName, settings.DataDirectoryName);
            }

            pathToDatabase = fileSystemAccessor.CombinePath(pathToDatabase, entityName + "-data.sqlite3");
            return pathToDatabase;
        }
        
        public SqlitePlainStorageWithWorkspace(SQLiteConnectionWithLock storage, ILogger logger)
            :base(storage, logger)
        {
            
        }
    }
}
