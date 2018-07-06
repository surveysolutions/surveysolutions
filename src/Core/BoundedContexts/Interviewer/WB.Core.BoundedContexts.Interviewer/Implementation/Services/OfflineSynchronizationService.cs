using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ncqrs.Eventing;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Questionnaire.Api;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class OfflineSynchronizationService : ISynchronizationService
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

        public Task UploadInterviewAsync(Guid interviewId, InterviewPackageApiView completedInterview,
            Action<decimal, long, long> onDownloadProgressChanged, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task UploadInterviewImageAsync(Guid interviewId, string fileName, byte[] fileData, Action<decimal, long, long> onDownloadProgressChanged,
            CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task UploadInterviewAudioAsync(Guid interviewId, string fileName, string contentType, byte[] fileData,
            Action<decimal, long, long> onDownloadProgressChanged, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<List<string>> GetAttachmentContentsAsync(QuestionnaireIdentity questionnaire, Action<decimal, long, long> onDownloadProgressChanged,
            CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<AttachmentContent> GetAttachmentContentAsync(string contentId, Action<decimal, long, long> onDownloadProgressChanged, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<List<QuestionnaireIdentity>> GetServerQuestionnairesAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<List<TranslationDto>> GetQuestionnaireTranslationAsync(QuestionnaireIdentity questionnaireIdentity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<CompanyLogoInfo> GetCompanyLogo(string storedClientEtag, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SendSyncStatisticsAsync(SyncStatisticsApiView statistics, CancellationToken token, RestCredentials credentials)
        {
            throw new NotImplementedException();
        }

        public Task SendUnexpectedExceptionAsync(UnexpectedExceptionApiView exception, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<List<MapView>> GetMapList(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<RestStreamResult> GetMapContentStream(string mapName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Guid> GetCurrentSupervisor(CancellationToken token, RestCredentials credentials)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsAutoUpdateEnabledAsync(CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task UploadAuditLogEntityAsync(AuditLogEntitiesApiView auditLogEntity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<List<Guid>> CheckObsoleteInterviewsAsync(List<ObsoletePackageCheck> checks, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
