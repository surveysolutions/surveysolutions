using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Ncqrs.Eventing;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Utils;
using DownloadProgressChangedEventArgs = WB.Core.GenericSubdomains.Portable.Implementation.DownloadProgressChangedEventArgs;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    [ExcludeFromCodeCoverage]
    public partial class OfflineSynchronizationService : ISynchronizationService
    {
        private static Version interviewerBoundedContextVersion;

        public Task<string> LoginAsync(LogonInfo logonInfo, RestCredentials credentials, CancellationToken? token = null)
        {
            return Task.FromResult("offline sync token");
        }

        public Task<bool> HasCurrentUserDeviceAsync(RestCredentials credentials = null, CancellationToken? token = null)
        {
            return Task.FromResult(true);
        }

        public async Task CanSynchronizeAsync(RestCredentials credentials = null, CancellationToken? token = null)
        {
            if (interviewerBoundedContextVersion == null)
            {
                interviewerBoundedContextVersion = 
                    ReflectionUtils.GetAssemblyVersion(typeof(InterviewerBoundedContextAssemblyIndicator));
            }

            var request = new CanSynchronizeRequest(interviewerBoundedContextVersion.Revision);
            var response = await this.syncClient.SendAsync<CanSynchronizeRequest, CanSynchronizeResponse>(request, 
                token ?? CancellationToken.None);
            if (!response.CanSyncronize)
            {
                throw new SynchronizationException(SynchronizationExceptionType.UpgradeRequired);
            }
        }

        public Task SendDeviceInfoAsync(DeviceInfoApiView info, CancellationToken? token = null)
        {
            throw new NotImplementedException();
        }

        public Task LinkCurrentUserToDeviceAsync(RestCredentials credentials = null, CancellationToken? token = null)
        {
            return Task.CompletedTask;
        }

        public async Task<byte[]> GetQuestionnaireAssemblyAsync(QuestionnaireIdentity questionnaire, Action<decimal, long, long> onDownloadProgressChanged,
            CancellationToken token)
        {
            var response = await this.syncClient.SendAsync<GetQuestionnaireAssemblyRequest, GetQuestionnaireAssemblyResponse>(
                new GetQuestionnaireAssemblyRequest(questionnaire), token);
            return response.Content;
        }

        public async Task<QuestionnaireApiView> GetQuestionnaireAsync(QuestionnaireIdentity questionnaire, Action<decimal, long, long> onDownloadProgressChanged,
            CancellationToken token)
        {
            var response = await this.syncClient.SendAsync<GetQuestionnaireRequest, GetQuestionnaireResponse>(
                new GetQuestionnaireRequest(questionnaire), token);
            return new QuestionnaireApiView
            {
                QuestionnaireDocument = response.QuestionnaireDocument
            };
        }

        public Task<List<QuestionnaireIdentity>> GetCensusQuestionnairesAsync(CancellationToken token)
        {
            return Task.FromResult(new List<QuestionnaireIdentity>());
        }

        public Task LogQuestionnaireAsSuccessfullyHandledAsync(QuestionnaireIdentity questionnaire)
        {
            return Task.CompletedTask;
        }

        public Task LogQuestionnaireAssemblyAsSuccessfullyHandledAsync(QuestionnaireIdentity questionnaire)
        {
            return Task.CompletedTask;
        }

        public Task<byte[]> GetApplicationAsync(CancellationToken token, Action<DownloadProgressChangedEventArgs> onDownloadProgressChanged = null)
        {
            return Task.FromResult<byte[]>(null);
        }

        public Task<byte[]> GetApplicationPatchAsync(CancellationToken token, Action<DownloadProgressChangedEventArgs> onDownloadProgressChanged = null)
        {
            return Task.FromResult<byte[]>(null);
        }

        public Task<int?> GetLatestApplicationVersionAsync(CancellationToken token)
        {
            return Task.FromResult<int?>(null);
        }

        public Task SendBackupAsync(string filePath, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        public async Task<List<InterviewApiView>> GetInterviewsAsync(CancellationToken token)
        {
            var response = await this.syncClient.SendAsync<GetInterviewsRequest, GetInterviewsResponse>(
                new GetInterviewsRequest(this.principal.CurrentUserIdentity.UserId), token);
            return null;
        }

        public Task LogInterviewAsSuccessfullyHandledAsync(Guid interviewId)
        {
            throw new NotImplementedException();
        }

        public Task<List<CommittedEvent>> GetInterviewDetailsAsync(Guid interviewId, Action<decimal, long, long> onDownloadProgressChanged, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
