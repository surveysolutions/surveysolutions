using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ncqrs.Eventing;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public partial class OfflineSynchronizationService : ISynchronizationService
    {
        public Task<string> LoginAsync(LogonInfo logonInfo, RestCredentials credentials, CancellationToken? token = null)
        {
            throw new NotImplementedException();
        }

        public Task<bool> HasCurrentUserDeviceAsync(RestCredentials credentials = null, CancellationToken? token = null)
        {
            throw new NotImplementedException();
        }

        public Task CanSynchronizeAsync(RestCredentials credentials = null, CancellationToken? token = null)
        {
            throw new NotImplementedException();
        }

        public Task SendDeviceInfoAsync(DeviceInfoApiView info, CancellationToken? token = null)
        {
            throw new NotImplementedException();
        }

        public Task LinkCurrentUserToDeviceAsync(RestCredentials credentials = null, CancellationToken? token = null)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> GetQuestionnaireAssemblyAsync(QuestionnaireIdentity questionnaire, Action<decimal, long, long> onDownloadProgressChanged,
            CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<QuestionnaireApiView> GetQuestionnaireAsync(QuestionnaireIdentity questionnaire, Action<decimal, long, long> onDownloadProgressChanged,
            CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<List<QuestionnaireIdentity>> GetCensusQuestionnairesAsync(CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task LogQuestionnaireAsSuccessfullyHandledAsync(QuestionnaireIdentity questionnaire)
        {
            throw new NotImplementedException();
        }

        public Task LogQuestionnaireAssemblyAsSuccessfullyHandledAsync(QuestionnaireIdentity questionnaire)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> GetApplicationAsync(CancellationToken token, Action<DownloadProgressChangedEventArgs> onDownloadProgressChanged = null)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> GetApplicationPatchAsync(CancellationToken token, Action<DownloadProgressChangedEventArgs> onDownloadProgressChanged = null)
        {
            throw new NotImplementedException();
        }

        public Task<int?> GetLatestApplicationVersionAsync(CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task SendBackupAsync(string filePath, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<List<InterviewApiView>> GetInterviewsAsync(CancellationToken token)
        {
            throw new NotImplementedException();
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
