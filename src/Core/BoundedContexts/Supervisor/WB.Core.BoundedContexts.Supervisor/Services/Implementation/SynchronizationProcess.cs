using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.BoundedContexts.Supervisor.Services
{
    public class SynchronizationProcess : ISynchronizationProcess
    {


        public Task SyncronizeAsync(IProgress<SyncProgressInfo> progress, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
