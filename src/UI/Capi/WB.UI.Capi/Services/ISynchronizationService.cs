using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CAPI.Android.Core.Model.Authorization;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace WB.UI.Capi.Services
{
    public interface ISynchronizationService
    {
        Task<Guid> HandshakeAsync(SyncCredentials credentials, CancellationToken token = default(CancellationToken));
        Task<IEnumerable<SynchronizationChunkMeta>> GetChunksAsync(SyncCredentials credentials, CancellationToken token, Guid? lastKnownPackageId);
        Task<SyncItem> RequestChunkAsync(SyncCredentials credentials, Guid chunkId, CancellationToken token);
        Task PushChunkAsync(SyncCredentials credentials, string chunkAsString, CancellationToken token);
        Task PushBinaryAsync(SyncCredentials credentials, Guid interviewId, string fileName, byte[] fileData, CancellationToken token);
        Task<Guid> GetChunkIdByTimestamp(SyncCredentials credentials, long timestamp, CancellationToken token);
        Task<bool> NewVersionAvailableAsync(CancellationToken token = default(CancellationToken));
    }
}
