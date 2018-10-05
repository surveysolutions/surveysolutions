using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class SynchronizationService : EnumeratorSynchronizationService, IInterviewerSynchronizationService
    {
        protected override string ApiVersion => "v2";
        protected override string ApiUrl => "api/interviewer/";

        protected override string InterviewsController => string.Concat(ApiUrl, "v3", "/interviews");

        public SynchronizationService(IPrincipal principal, IRestService restService,
            IInterviewerSettings interviewerSettings, IInterviewerSyncProtocolVersionProvider syncProtocolVersionProvider,
            IFileSystemAccessor fileSystemAccessor, ICheckVersionUriProvider checkVersionUriProvider, ILogger logger) :
            base(principal, restService, interviewerSettings, syncProtocolVersionProvider, fileSystemAccessor,
                checkVersionUriProvider, logger, interviewerSettings)
        {
        }

        public Task<InterviewerApiView> GetInterviewerAsync(RestCredentials credentials = null, CancellationToken? token = null)
        {
            return this.TryGetRestResponseOrThrowAsync(() =>
                this.restService.GetAsync<InterviewerApiView>(url: string.Concat(this.UsersController, "/current"),
                    credentials: credentials ?? this.restCredentials, token: token));
        }

        public Task<List<QuestionnaireIdentity>> GetCensusQuestionnairesAsync(CancellationToken token)
        {
            return this.TryGetRestResponseOrThrowAsync(() => this.restService.GetAsync<List<QuestionnaireIdentity>>(
                url: string.Concat(this.QuestionnairesController, "/census"),
                credentials: this.restCredentials, token: token));
        }

        public Task<Guid> GetCurrentSupervisor(CancellationToken token, RestCredentials credentials)
        {
            return this.TryGetRestResponseOrThrowAsync(() =>
                this.restService.GetAsync<Guid>(url: string.Concat(this.UsersController, "/supervisor"),
                    credentials: credentials ?? this.restCredentials, token: token));
        }

        protected override string CanSynchronizeValidResponse => "449634775";
    }
}
