using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Supervisor.Views;
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
        protected string ExceptionsController => string.Concat(ApplicationUrl, "/interviewerExceptions");
        protected string BrokenInterviewPackagesController => string.Concat(ApplicationUrl, "/brokenInterviews");
        protected string InterviewerTabletInfosController => string.Concat(ApplicationUrl, "/interviewerTabletInfos");
        protected string InterviewerStatisticsController => string.Concat(ApplicationUrl, "/interviewerStatistics");
        protected string GetListOfDeletedQuestionnairesController => string.Concat(ApplicationUrl, "/deletedQuestionnairesList");

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

        public Task UploadBrokenInterviewPackageAsync(BrokenInterviewPackageApiView brokenInterviewPackage,
            CancellationToken cancellationToken)
        {
            return this.TryGetRestResponseOrThrowAsync(() => this.restService.PostAsync(
                url: this.BrokenInterviewPackagesController,
                request: brokenInterviewPackage,
                credentials: this.restCredentials,
                token: cancellationToken));
        }

        public Task UploadInterviewerExceptionsAsync(List<UnexpectedExceptionFromInterviewerView> exceptions, CancellationToken cancellationToken)
        {
            return this.TryGetRestResponseOrThrowAsync(() => this.restService.PostAsync(
                url: this.ExceptionsController,
                request: exceptions,
                credentials: this.restCredentials,
                token: cancellationToken));
        }

        public Task UploadTabletInfoAsync(DeviceInfoApiView deviceInfoApiView,
            CancellationToken cancellationToken)
        {
            return this.TryGetRestResponseOrThrowAsync(() => this.restService.PostAsync(
                url: this.InterviewerTabletInfosController,
                request: deviceInfoApiView,
                credentials: this.restCredentials,
                token: cancellationToken));
        }

        public Task UploadInterviewerSyncStatistic(InterviewerSyncStatisticsApiView statisticToSend, CancellationToken cancellationToken)
        {
            return this.TryGetRestResponseOrThrowAsync(() => this.restService.PostAsync(
                url: this.InterviewerStatisticsController,
                request: statisticToSend,
                credentials: this.restCredentials,
                token: cancellationToken));
        }

        public Task<List<string>> GetListOfDeletedQuestionnairesIds(CancellationToken cancellationToken)
        {
            return this.TryGetRestResponseOrThrowAsync(() => this.restService.GetAsync<List<string>>(
                url: this.GetListOfDeletedQuestionnairesController,              
                credentials: this.restCredentials,
                token: cancellationToken));
        }

        public async Task<byte[]> GetInterviewerApplicationAsync(byte[] existingFileHash, CancellationToken token, IProgress<TransferProgress> transferProgress = null) =>
            await this.TryGetRestResponseOrThrowAsync(async () =>
            {
                var restFile = await this.restService.DownloadFileAsync(
                    url: string.Concat(ApplicationUrl, "/apk/interviewer"), 
                    token: token,
                    credentials: this.restCredentials,
                    transferProgress: transferProgress,
                    customHeaders: new Dictionary<string, string>
                    {
                        ["If-None-Match"] = Convert.ToBase64String(existingFileHash)
                    });

                return restFile.Content;
            });

        public async Task<byte[]> GetInterviewerApplicationWithMapsAsync(byte[] existingFileHash,CancellationToken token, 
            IProgress<TransferProgress> transferProgress = null) =>
            await this.TryGetRestResponseOrThrowAsync(async () =>
            {
                var restFile = await this.restService.DownloadFileAsync(
                    url: string.Concat(ApplicationUrl, "/apk/interviewer-with-maps"), 
                    token: token,
                    credentials: this.restCredentials,
                    transferProgress: transferProgress,
                    customHeaders: new Dictionary<string, string>
                    {
                        ["If-None-Match"] = Convert.ToBase64String(existingFileHash)
                    });

                return restFile.Content;
            });

        protected override string CanSynchronizeValidResponse => "158329303";
    }
}
