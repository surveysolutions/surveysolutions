using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using CAPI.Android.Core.Model.Authorization;

using Main.Core.Entities.SubEntities;

using WB.Core.BoundedContexts.Capi;

namespace CAPI.Android.Core.Model
{
    public interface IDataCollectionAuthentication : IAuthentication
    {
        SyncCredentials? RequestSyncCredentials();

        Task<List<UserLight>> GetKnownUsers();

        Task<Guid?> GetUserIdByLoginIfExists(string login);
    }
}