using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CAPI.Android.Core.Model.Authorization;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace WB.UI.Capi.Services
{
    public interface ISynchronizationService
    {
        Task<Guid> HandshakeAsync(SyncCredentials credentials);
        Task<IEnumerable<SynchronizationChunkMeta>> GetChunksAsync(SyncCredentials credentials, string lastKnownPackageId);
        Task<SyncItem> RequestChunkAsync(SyncCredentials credentials, string chunkId);
        Task PushChunkAsync(SyncCredentials credentials, Guid interviewId, string chunkAsString);
        Task PushBinaryAsync(SyncCredentials credentials, Guid interviewId, string fileName, byte[] fileData);
        Task<string> GetChunkIdByTimestampAsync(SyncCredentials credentials, long timestamp);
        Task<bool> NewVersionAvailableAsync();
    }
}
