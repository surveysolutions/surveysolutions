using System.Threading;
using System.Threading.Tasks;

using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class RemoteAuthorizationService : RestBaseService, IRemoteAuthorizationService
    {
        private const string interviewerApiUrl = "api/interviewer/v1";
        private readonly string usersController = string.Concat(interviewerApiUrl, "/users");

        private readonly IPrincipal principal;
        private readonly IRestService restService;

        private RestCredentials restCredentials
        {
            get
            {
                return this.principal.CurrentUserIdentity == null
                    ? null
                    : new RestCredentials { Login = this.principal.CurrentUserIdentity.Name, Password = this.principal.CurrentUserIdentity.Password };
            }
        }

        public RemoteAuthorizationService(
            IPrincipal principal, 
            IRestService restService, 
            ILogger logger) : base(logger)
        {
            this.principal = principal;
            this.restService = restService;
        }

        public async Task<InterviewerApiView> GetInterviewerAsync(RestCredentials credentials = null, CancellationToken? token = null)
        {
            return await this.TryGetRestResponseOrThrowAsync(async () => await this.restService.GetAsync<InterviewerApiView>(url: string.Concat(this.usersController, "/current"),
                credentials: credentials ?? this.restCredentials, token: token));

        }
    }
}