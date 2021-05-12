using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ncqrs.Eventing;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.Infrastructure.HttpServices.HttpClient;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Questionnaire.Api;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.SurveySolutions.ReusableCategories;

namespace WB.Core.SharedKernels.Enumerator.Services.Synchronization
{
    public interface ISynchronizationService
    {
        Task<string> LoginAsync(LogonInfo logonInfo, RestCredentials credentials, CancellationToken token = default);
        Task<string> ChangePasswordAsync(ChangePasswordInfo info, CancellationToken token = default);
        Task<bool> HasCurrentUserDeviceAsync(RestCredentials credentials = null, CancellationToken token = default);
        Task<string> GetTenantId(RestCredentials credentials = null, CancellationToken token = default);

        Task CanSynchronizeAsync(RestCredentials credentials = null, string tenantId = null, CancellationToken token = default);
        Task SendDeviceInfoAsync(DeviceInfoApiView info, CancellationToken token = default);
        Task LinkCurrentUserToDeviceAsync(RestCredentials credentials = null, CancellationToken token = default);

        Task<byte[]> GetQuestionnaireAssemblyAsync(QuestionnaireIdentity questionnaire, IProgress<TransferProgress> transferProgress, CancellationToken token = default);
        Task<QuestionnaireApiView> GetQuestionnaireAsync(QuestionnaireIdentity questionnaire, IProgress<TransferProgress> transferProgress, CancellationToken token = default);
        Task LogQuestionnaireAsSuccessfullyHandledAsync(QuestionnaireIdentity questionnaire);
        Task LogQuestionnaireAssemblyAsSuccessfullyHandledAsync(QuestionnaireIdentity questionnaire);

        Task<byte[]> GetApplicationAsync(IProgress<TransferProgress> transferProgress = null, CancellationToken token = default);
        Task<byte[]> GetApplicationPatchAsync(IProgress<TransferProgress> transferProgress = null, CancellationToken token = default);
        Task<int?> GetLatestApplicationVersionAsync(CancellationToken token = default);

        Task<List<InterviewApiView>> GetInterviewsAsync(CancellationToken token = default);

        Task LogInterviewAsSuccessfullyHandledAsync(Guid interviewId);

        Task<List<CommittedEvent>> GetInterviewDetailsAsync(Guid interviewId, IProgress<TransferProgress> transferProgress, CancellationToken token = default);
        Task<List<CommittedEvent>> GetInterviewDetailsAfterEventAsync(Guid interviewId, Guid eventId, IProgress<TransferProgress> transferProgress, CancellationToken token = default);
        Task<InterviewUploadState> GetInterviewUploadState(Guid interviewId, EventStreamSignatureTag eventStreamSignatureTag, CancellationToken token = default);
        Task UploadInterviewAsync(Guid interviewId, InterviewPackageApiView completedInterview, IProgress<TransferProgress> transferProgress, CancellationToken token = default);
        Task UploadInterviewImageAsync(Guid interviewId, string fileName, byte[] fileData, IProgress<TransferProgress> transferProgress, CancellationToken token = default);
        Task UploadInterviewAudioAsync(Guid interviewId, string fileName, string contentType, byte[] fileData, IProgress<TransferProgress> transferProgressd, CancellationToken token = default);
        Task UploadInterviewAudioAuditAsync(Guid interviewId, string fileName, string contentType, byte[] fileData, IProgress<TransferProgress> transferProgress, CancellationToken token = default);
        Task<List<string>> GetAttachmentContentsAsync(QuestionnaireIdentity questionnaire, IProgress<TransferProgress> transferProgress, CancellationToken token = default);
        Task<AttachmentContent> GetAttachmentContentAsync(string contentId, IProgress<TransferProgress> transferProgress, CancellationToken token = default);
        Task<List<QuestionnaireIdentity>> GetServerQuestionnairesAsync(CancellationToken token = default);
        
        Task<List<QuestionnaireIdentity>> GetServerQuestionnairesPermittedToSwitchToWebModeAsync(CancellationToken token = default);

        Task<List<TranslationDto>> GetQuestionnaireTranslationAsync(QuestionnaireIdentity questionnaireIdentity, CancellationToken token = default);

        Task<CompanyLogoInfo> GetCompanyLogo(string storedClientEtag, CancellationToken cancellationToken);
        Task<long?> SendSyncStatisticsAsync(SyncStatisticsApiView statistics, RestCredentials credentials, CancellationToken token = default);
        Task SendUnexpectedExceptionAsync(UnexpectedExceptionApiView exception, CancellationToken token);

        Task<List<MapView>> GetMapList(CancellationToken cancellationToken);
        Task<RestStreamResult> GetMapContentStream(string mapName, CancellationToken cancellationToken);

        Task<bool> IsAutoUpdateEnabledAsync(CancellationToken token);
        Task UploadAuditLogEntityAsync(AuditLogEntitiesApiView auditLogEntity, CancellationToken cancellationToken);
        Task<List<Guid>> CheckObsoleteInterviewsAsync(List<ObsoletePackageCheck> checks, CancellationToken cancellationToken);
        Task<AssignmentApiDocument> GetAssignmentAsync(int id, CancellationToken cancellationToken);
        Task<List<AssignmentApiView>> GetAssignmentsAsync(CancellationToken cancellationToken);
        Task LogAssignmentAsHandledAsync(int id, CancellationToken cancellationToken);
        Task<string> GetPublicKeyForEncryptionAsync(CancellationToken cancellationToken);
        Task<bool> AreNotificationsEnabledAsync(CancellationToken token);
        Task<List<ReusableCategoriesDto>> GetQuestionnaireReusableCategoriesAsync(QuestionnaireIdentity questionnaireIdentity, CancellationToken cancellationToken);
        Task<RemoteTabletSettingsApiView> GetTabletSettings(CancellationToken cancellationToken);

        Task UploadCalendarEventAsync(Guid calendarEventId, CalendarEventPackageApiView completedInterview,
            IProgress<TransferProgress> transferProgress, CancellationToken token);

        Task<List<CommittedEvent>> GetCalendarEventStreamAsync(Guid calendarEventId, int? sequence,
            IProgress<TransferProgress> transferProgress, CancellationToken token);
        
        Task<List<CalendarEventApiView>> GetCalendarEventsAsync(CancellationToken token = default);
    }
}
