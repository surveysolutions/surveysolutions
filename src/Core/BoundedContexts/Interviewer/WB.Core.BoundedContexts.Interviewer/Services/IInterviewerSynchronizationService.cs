using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.Infrastructure.HttpServices.HttpClient;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;

namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public interface IInterviewerSynchronizationService : ISynchronizationService
    {
        Task<InterviewerApiView> GetInterviewerAsync(RestCredentials? credentials = null, CancellationToken token = default);
        Task<Guid> GetCurrentSupervisor(RestCredentials credentials, CancellationToken token = default);
    }
}
