using System;
using System.Threading;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public interface IMapSyncProvider
    {
        Task SyncronizeAsync(IProgress<SyncProgressInfo> progress, CancellationToken cancellationToken);
    }
}