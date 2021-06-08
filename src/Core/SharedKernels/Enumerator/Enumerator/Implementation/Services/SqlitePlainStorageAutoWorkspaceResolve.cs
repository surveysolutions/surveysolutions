using System.Collections.Generic;
using System.Data.Common;
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

        private readonly Dictionary<string, SQLiteConnectionWithLock> connections = new();

        protected override SQLiteConnectionWithLock GetConnection()
        {
            var workspaceName = NonWorkspaced
                ? NonWorkspacedName
                : workspaceAccessor.GetCurrentWorkspaceName() ?? "primary";
            logger.Trace($"Request on connection for {typeof(TEntity).Name} for {workspaceName}");
            if (connections.TryGetValue(workspaceName, out var connection))
            {
                logger.Trace($"Get saved connection for {typeof(TEntity).Name} for {workspaceName}");
                return connection;
            }

            logger.Trace($"Create connection for {typeof(TEntity).Name} for {workspaceName}");
            var newConnection = base.CreateConnection();
            connections[workspaceName] = newConnection;
            logger.Trace($"Created and store connection for {typeof(TEntity).Name} for {workspaceName}");
            return newConnection;
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