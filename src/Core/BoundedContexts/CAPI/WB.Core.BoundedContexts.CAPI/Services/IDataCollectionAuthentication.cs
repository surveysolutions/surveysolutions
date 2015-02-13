using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Main.Core.Entities.SubEntities;

using WB.Core.BoundedContexts.Capi.Implementation.Authorization;

namespace WB.Core.BoundedContexts.Capi.Services
{
    public interface IDataCollectionAuthentication : IAuthentication
    {
        SyncCredentials? RequestSyncCredentials();

        Task<List<UserLight>> GetKnownUsers();

        Task<Guid?> GetUserIdByLoginIfExists(string login);
    }
}