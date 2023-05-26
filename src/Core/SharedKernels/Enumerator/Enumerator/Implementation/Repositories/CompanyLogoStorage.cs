#nullable enable
using SQLite;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Workspace;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Repositories;

public class CompanyLogoStorage : SqlitePlainStorageAutoWorkspaceResolve<CompanyLogo>, ICompanyLogoStorage
{
    public CompanyLogoStorage(ILogger logger, IFileSystemAccessor fileSystemAccessor, SqliteSettings settings, 
        IWorkspaceAccessor workspaceAccessor)
        : base(logger, fileSystemAccessor, settings, workspaceAccessor)
    {
    }

    public CompanyLogoStorage(SQLiteConnectionWithLock storage, ILogger logger) : base(storage, logger)
    {
    }
    
    public CompanyLogo? GetCompanyLogoByWorkspace(string keyName, string workspace)
    {
        return this.GetById(keyName);
        // return this.GetConnection().Table<CompanyLogo>();
        //
        //
        //
        // var connect = GetConnection();
        //
        // TResult result = default(TResult);
        // using (connect.Lock())
        //     connect.RunInTransaction(() => result = function.Invoke(connect.Table<TEntity>()));
        // return result;
        
        
    }
    
}
