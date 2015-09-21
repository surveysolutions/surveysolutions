using System.Threading;
using System.Threading.Tasks;

using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.DataCollection.WebApi;

namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public interface IRegistrationService
    {
        Task<InterviewerApiView> GetInterviewerAsync(RestCredentials credentials = null, CancellationToken? token = null);
        Task<bool> HasCurrentInterviewerDeviceAsync(RestCredentials credentials = null, CancellationToken? token = null);

        Task<bool> IsDeviceLinkedToCurrentInterviewerAsync(RestCredentials credentials = null, CancellationToken? token = null);
        Task LinkCurrentInterviewerToDeviceAsync(RestCredentials credentials = null, CancellationToken? token = null);
    }
}