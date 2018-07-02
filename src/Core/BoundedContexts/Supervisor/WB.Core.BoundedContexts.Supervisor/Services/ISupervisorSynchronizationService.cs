using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;

namespace WB.Core.BoundedContexts.Supervisor.Services
{
    public interface ISupervisorSynchronizationService : ISynchronizationService
    {
        Task<SupervisorApiView> GetSupervisorAsync(RestCredentials credentials = null, CancellationToken? token = null);
        Task<List<InterviewerFullApiView>> GetInterviewersAsync(CancellationToken cancellationToken);
    }
}
