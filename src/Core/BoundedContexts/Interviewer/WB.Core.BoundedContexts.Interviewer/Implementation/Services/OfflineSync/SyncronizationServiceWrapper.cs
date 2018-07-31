using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Ncqrs.Eventing;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Questionnaire.Api;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    [ExcludeFromCodeCoverage]
    public class SyncronizationServiceWrapper : ISynchronizationService
    {
        private readonly OfflineSynchronizationService offlineService;
        private readonly SynchronizationService onlineService;
        private readonly IInterviewerSettings interviewerSettings;

        public SyncronizationServiceWrapper(
            OfflineSynchronizationService offlineService, 
            SynchronizationService onlineService, IInterviewerSettings interviewerSettings)
        {
            this.offlineService = offlineService;
            this.onlineService = onlineService;
            this.interviewerSettings = interviewerSettings;
        }

        private ISynchronizationService Service => this.interviewerSettings.AllowSyncWithHq
            ? this.onlineService
            : (ISynchronizationService) this.offlineService;

        public Task<string> LoginAsync(LogonInfo logonInfo, RestCredentials credentials, CancellationToken? token = null)
        {
            return Service.LoginAsync(logonInfo, credentials, token);
        }

        public Task<bool> HasCurrentUserDeviceAsync(RestCredentials credentials = null, CancellationToken? token = null)
        {
            return Service.HasCurrentUserDeviceAsync(credentials, token);
        }

        public Task CanSynchronizeAsync(RestCredentials credentials = null, CancellationToken? token = null)
        {
            return Service.CanSynchronizeAsync(credentials, token);
        }

        public Task SendDeviceInfoAsync(DeviceInfoApiView info, CancellationToken? token = null)
        {
            return Service.SendDeviceInfoAsync(info, token);
        }

        public Task LinkCurrentUserToDeviceAsync(RestCredentials credentials = null, CancellationToken? token = null)
        {
            return Service.LinkCurrentUserToDeviceAsync(credentials, token);
        }

        public Task<byte[]> GetQuestionnaireAssemblyAsync(QuestionnaireIdentity questionnaire, IProgress<TransferProgress> transferProgress,
            CancellationToken token)
        {
            return Service.GetQuestionnaireAssemblyAsync(questionnaire, transferProgress, token);
        }

        public Task<QuestionnaireApiView> GetQuestionnaireAsync(QuestionnaireIdentity questionnaire, IProgress<TransferProgress> transferProgress,
            CancellationToken token)
        {
            return Service.GetQuestionnaireAsync(questionnaire, transferProgress, token);
        }

        public Task<List<QuestionnaireIdentity>> GetCensusQuestionnairesAsync(CancellationToken token)
        {
            return Service.GetCensusQuestionnairesAsync(token);
        }

        public Task LogQuestionnaireAsSuccessfullyHandledAsync(QuestionnaireIdentity questionnaire)
        {
            return Service.LogQuestionnaireAsSuccessfullyHandledAsync(questionnaire);
        }

        public Task LogQuestionnaireAssemblyAsSuccessfullyHandledAsync(QuestionnaireIdentity questionnaire)
        {
            return Service.LogQuestionnaireAssemblyAsSuccessfullyHandledAsync(questionnaire);
        }

        public Task<byte[]> GetApplicationAsync(CancellationToken token, IProgress<TransferProgress> transferProgress = null)
        {
            return Service.GetApplicationAsync(token, transferProgress);
        }

        public Task<byte[]> GetApplicationPatchAsync(CancellationToken token, IProgress<TransferProgress> transferProgress = null)
        {
            return Service.GetApplicationPatchAsync(token, transferProgress);
        }

        public Task<int?> GetLatestApplicationVersionAsync(CancellationToken token)
        {
            return Service.GetLatestApplicationVersionAsync(token);
        }

        public Task SendBackupAsync(string filePath, CancellationToken token)
        {
            return Service.SendBackupAsync(filePath, token);
        }

        public Task<List<InterviewApiView>> GetInterviewsAsync(CancellationToken token)
        {
            return Service.GetInterviewsAsync(token);
        }

        public Task LogInterviewAsSuccessfullyHandledAsync(Guid interviewId)
        {
            return Service.LogInterviewAsSuccessfullyHandledAsync(interviewId);
        }

        public Task<List<CommittedEvent>> GetInterviewDetailsAsync(Guid interviewId, IProgress<TransferProgress> transferProgress, CancellationToken token)
        {
            return Service.GetInterviewDetailsAsync(interviewId, transferProgress, token);
        }

        public Task UploadInterviewAsync(Guid interviewId, InterviewPackageApiView completedInterview,
            IProgress<TransferProgress> transferProgress, CancellationToken token)
        {
            return Service.UploadInterviewAsync(interviewId, completedInterview, transferProgress, token);
        }

        public Task UploadInterviewImageAsync(Guid interviewId, string fileName, byte[] fileData, IProgress<TransferProgress> transferProgress,
            CancellationToken token)
        {
            return Service.UploadInterviewImageAsync(interviewId, fileName, fileData, transferProgress, token);
        }

        public Task UploadInterviewAudioAsync(Guid interviewId, string fileName, string contentType, byte[] fileData,
            IProgress<TransferProgress> transferProgress, CancellationToken token)
        {
            return Service.UploadInterviewAudioAsync(interviewId, fileName, contentType, fileData,
                transferProgress, token);
        }

        public Task<List<string>> GetAttachmentContentsAsync(QuestionnaireIdentity questionnaire, IProgress<TransferProgress> transferProgress,
            CancellationToken token)
        {
            return Service.GetAttachmentContentsAsync(questionnaire, transferProgress, token);
        }

        public Task<AttachmentContent> GetAttachmentContentAsync(string contentId, IProgress<TransferProgress> transferProgress, CancellationToken token)
        {
            return Service.GetAttachmentContentAsync(contentId, transferProgress, token);
        }

        public Task<List<QuestionnaireIdentity>> GetServerQuestionnairesAsync(CancellationToken cancellationToken)
        {
            return Service.GetServerQuestionnairesAsync(cancellationToken);
        }

        public Task<List<TranslationDto>> GetQuestionnaireTranslationAsync(QuestionnaireIdentity questionnaireIdentity, CancellationToken cancellationToken)
        {
            return Service.GetQuestionnaireTranslationAsync(questionnaireIdentity, cancellationToken);
        }

        public Task<CompanyLogoInfo> GetCompanyLogo(string storedClientEtag, CancellationToken cancellationToken)
        {
            return Service.GetCompanyLogo(storedClientEtag, cancellationToken);
        }

        public Task<long?> SendSyncStatisticsAsync(SyncStatisticsApiView statistics, CancellationToken token, RestCredentials credentials)
        {
            return Service.SendSyncStatisticsAsync(statistics, token, credentials);
        }

        public Task SendUnexpectedExceptionAsync(UnexpectedExceptionApiView exception, CancellationToken token)
        {
            return Service.SendUnexpectedExceptionAsync(exception, token);
        }

        public Task<List<MapView>> GetMapList(CancellationToken cancellationToken)
        {
            return Service.GetMapList(cancellationToken);
        }

        public Task<RestStreamResult> GetMapContentStream(string mapName, CancellationToken cancellationToken)
        {
            return Service.GetMapContentStream(mapName, cancellationToken);
        }

        public Task<Guid> GetCurrentSupervisor(CancellationToken token, RestCredentials credentials)
        {
            return Service.GetCurrentSupervisor(token, credentials);
        }

        public Task<bool> IsAutoUpdateEnabledAsync(CancellationToken token)
        {
            return Service.IsAutoUpdateEnabledAsync(token);
        }

        public Task UploadAuditLogEntityAsync(AuditLogEntitiesApiView auditLogEntity, CancellationToken cancellationToken)
        {
            return Service.UploadAuditLogEntityAsync(auditLogEntity, cancellationToken);
        }

        public Task<List<Guid>> CheckObsoleteInterviewsAsync(List<ObsoletePackageCheck> checks, CancellationToken cancellationToken)
        {
            return Service.CheckObsoleteInterviewsAsync(checks, cancellationToken);
        }

        public Task<AssignmentApiDocument> GetAssignmentAsync(int id, CancellationToken cancellationToken)
        {
            return Service.GetAssignmentAsync(id, cancellationToken);
        }

        public Task<List<AssignmentApiView>> GetAssignmentsAsync(CancellationToken cancellationToken)
        {
            return Service.GetAssignmentsAsync(cancellationToken);
        }

        public Task LogAssignmentAsHandledAsync(int id, CancellationToken cancellationToken)
        {
            return Service.LogAssignmentAsHandledAsync(id, cancellationToken);
        }
    }
}
