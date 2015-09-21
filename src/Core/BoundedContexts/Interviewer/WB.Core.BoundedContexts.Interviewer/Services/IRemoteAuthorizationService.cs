using System.Threading;
using System.Threading.Tasks;

using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.DataCollection.WebApi;

namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public interface IRemoteAuthorizationService
    {
        Task<InterviewerApiView> GetInterviewerAsync(RestCredentials credentials = null, CancellationToken? token = null);
    }
}