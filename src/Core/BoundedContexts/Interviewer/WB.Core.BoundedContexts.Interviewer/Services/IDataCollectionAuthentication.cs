using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Interviewer.Implementation.Authorization;

namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public interface IDataCollectionAuthentication : IAuthentication
    {
        SyncCredentials? RequestSyncCredentials();

        Task<List<string>> GetKnownUsers();

        Task<Guid?> GetUserIdByLoginIfExists(string login);
    }
}