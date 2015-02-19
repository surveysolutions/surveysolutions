using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Capi.Implementation.Authorization;

namespace WB.Core.BoundedContexts.Capi.Services
{
    public interface IDataCollectionAuthentication : IAuthentication
    {
        SyncCredentials? RequestSyncCredentials();

        Task<List<string>> GetKnownUsers();

        Task<Guid?> GetUserIdByLoginIfExists(string login);
    }
}