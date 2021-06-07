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
        private bool NonWorkspaced;
        
        public SqlitePlainStorageAutoWorkspaceResolve(ILogger logger, IFileSystemAccessor fileSystemAccessor, SqliteSettings settings, IWorkspaceAccessor workspaceAccessor) 
            : base(logger, fileSystemAccessor, settings, workspaceAccessor)
        {
            NonWorkspaced = typeof(TEntity).GetCustomAttribute(typeof(NonWorkspacedAttribute)) != null;
        }

        public SqlitePlainStorageAutoWorkspaceResolve(SQLiteConnectionWithLock storage, ILogger logger) 
            : base(storage, logger)
        {
            NonWorkspaced = typeof(TEntity).GetCustomAttribute(typeof(NonWorkspacedAttribute)) != null;
        }

        private readonly Dictionary<string, SQLiteConnectionWithLock> connections = new();

        protected override SQLiteConnectionWithLock GetConnection()
        {
            var workspaceName = NonWorkspaced
                ? "NonWorkspaced"
                : workspaceAccessor.GetCurrentWorkspaceName() ?? "primary";
            if (connections.TryGetValue(workspaceName, out var connection))
                return connection;

            var newConnection = base.CreateConnection();
            connections[workspaceName] = newConnection;
            return newConnection;
        }

        public override void Dispose()
        {
            foreach (var connectionWithLock in connections)
                connectionWithLock.Value?.Dispose();
            
            base.Dispose();
        }
    }
}