using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.BoundedContexts.Supervisor.Services.Implementation
{
    public class SynchronizationService : EnumeratorSynchronizationService, ISupervisorSynchronizationService
    {
        protected override string ApiVersion => "v1";
        protected override string ApiUrl => "api/supervisor/";

        protected string InterviewersController => string.Concat(ApplicationUrl, "/interviewers");

        public SynchronizationService(IPrincipal principal, IRestService restService,
            ISupervisorSettings settings, ISupervisorSyncProtocolVersionProvider syncProtocolVersionProvider,
            IFileSystemAccessor fileSystemAccessor, ICheckVersionUriProvider checkVersionUriProvider, ILogger logger) :
            base(principal, restService, settings, syncProtocolVersionProvider, fileSystemAccessor,
                checkVersionUriProvider, logger, settings)
        {
        }

        public Task<SupervisorApiView> GetSupervisorAsync(RestCredentials credentials = null, CancellationToken? token = null)
        {
            return this.TryGetRestResponseOrThrowAsync(() =>
                this.restService.GetAsync<SupervisorApiView>(url: string.Concat(this.UsersController, "/current"),
                    credentials: credentials ?? this.restCredentials, token: token));
        }

        public Task<List<InterviewerFullApiView>> GetInterviewersAsync(CancellationToken cancellationToken)
        {
            var response = this.TryGetRestResponseOrThrowAsync(() => this.restService.GetAsync<List<InterviewerFullApiView>>(
                    url: this.InterviewersController, 
                    credentials: this.restCredentials, 
                    token: cancellationToken));

            return response;
        }

        protected override string CanSynchronizeValidResponse => "158329303";
    }
}
