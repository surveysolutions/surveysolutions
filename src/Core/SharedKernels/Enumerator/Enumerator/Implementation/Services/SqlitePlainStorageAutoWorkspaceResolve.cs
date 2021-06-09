using System.Collections.Concurrent;
using System.Reflection;
using SQLite;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Workspace;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class SqlitePlainStorageAutoWorkspaceResolve<TEntity> : SqlitePlainStorageAutoWorkspaceResolve<TEntity, string>, IPlainStorage<TEntity>
        where TEntity : class, IPlainStorageEntity, new()
    {
        public SqlitePlainStorageAutoWorkspaceResolve(ILogger logger, IFileSystemAccessor fileSystemAccessor, SqliteSettings settings,
            IWorkspaceAccessor workspaceAccessor) 
            : base(logger, fileSystemAccessor, settings, workspaceAccessor)
        {
        }

        public SqlitePlainStorageAutoWorkspaceResolve(SQLiteConnectionWithLock storage, ILogger logger) : base(storage, logger)
        {
        }
    }
    
    public class SqlitePlainStorageAutoWorkspaceResolve<TEntity, TKey> : SqlitePlainStorageWithWorkspace<TEntity, TKey>
        where TEntity : class, IPlainStorageEntity<TKey>, new()
    {
        private const string NonWorkspacedName = "NonWorkspaced";
        private bool NonWorkspaced;
        
        public SqlitePlainStorageAutoWorkspaceResolve(ILogger logger, IFileSystemAccessor fileSystemAccessor, SqliteSettings settings, IWorkspaceAccessor workspaceAccessor) 
            : base(logger, fileSystemAccessor, settings, workspaceAccessor)
        {
            NonWorkspaced = typeof(TEntity).GetCustomAttribute(typeof(NonWorkspacedAttribute)) != null;
        }

        public SqlitePlainStorageAutoWorkspaceResolve(SQLiteConnectionWithLock storage, ILogger logger) 
            : base(storage, logger)
        {
            NonWorkspaced = true;
            connections[NonWorkspacedName] = storage;
        }

        private static readonly ConcurrentDictionary<string, SQLiteConnectionWithLock> connections = new();

        protected override SQLiteConnectionWithLock GetConnection()
        {
            var workspaceName = GetWorkspaceName();

            //logger.Trace($"Requesting connection for {typeof(TEntity).Name} in {workspaceName}");

            return connections.GetOrAdd(workspaceName, valueFactory: (string ws) =>
            {
                //logger.Trace($"Creating connection for {typeof(TEntity).Name} in {workspaceName}");
                return base.CreateConnection();
            });
        }

        private string GetWorkspaceName()
        {
            return NonWorkspaced
                ? NonWorkspacedName
                : workspaceAccessor.GetCurrentWorkspaceName() ?? "primary";
        }

        protected override string GetPathToDatabase()
        {
            string pathToDatabase = 
                NonWorkspaced ?
                settings.PathToDatabaseDirectory:
                fileSystemAccessor.CombinePath(settings.PathToRootDirectory, GetWorkspaceName(), settings.DataDirectoryName);
            
            pathToDatabase = fileSystemAccessor.CombinePath(pathToDatabase, typeof(TEntity).Name + "-data.sqlite3");
            return pathToDatabase;
        }

        public override void Dispose()
        {
            foreach (var connectionWithLock in connections)
                connectionWithLock.Value?.Dispose();
            
            connections.Clear();
            base.Dispose();
        }
    }
}
