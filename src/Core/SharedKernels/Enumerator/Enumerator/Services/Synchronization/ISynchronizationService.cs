using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ncqrs.Eventing;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Questionnaire.Api;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.SharedKernels.Enumerator.Services.Synchronization
{
    public interface ISynchronizationService
    {
        Task<string> LoginAsync(LogonInfo logonInfo, RestCredentials credentials, CancellationToken? token = null);
        Task<bool> HasCurrentUserDeviceAsync(RestCredentials credentials = null, CancellationToken? token = null);

        Task CanSynchronizeAsync(RestCredentials credentials = null, CancellationToken? token = null);
        Task SendDeviceInfoAsync(DeviceInfoApiView info, CancellationToken? token = null);
        Task LinkCurrentUserToDeviceAsync(RestCredentials credentials = null, CancellationToken? token = null);

        Task<byte[]> GetQuestionnaireAssemblyAsync(QuestionnaireIdentity questionnaire, IProgress<TransferProgress> transferProgress, CancellationToken token);
        Task<QuestionnaireApiView> GetQuestionnaireAsync(QuestionnaireIdentity questionnaire, IProgress<TransferProgress> transferProgress, CancellationToken token);
        Task<List<QuestionnaireIdentity>> GetCensusQuestionnairesAsync(CancellationToken token);
        Task LogQuestionnaireAsSuccessfullyHandledAsync(QuestionnaireIdentity questionnaire);
        Task LogQuestionnaireAssemblyAsSuccessfullyHandledAsync(QuestionnaireIdentity questionnaire);

        Task<byte[]> GetApplicationAsync(CancellationToken token, IProgress<TransferProgress> transferProgress = null);
        Task<byte[]> GetApplicationPatchAsync(CancellationToken token, IProgress<TransferProgress> transferProgress = null);
        Task<int?> GetLatestApplicationVersionAsync(CancellationToken token);
        Task SendBackupAsync(string filePath, CancellationToken token);

        Task<List<InterviewApiView>> GetInterviewsAsync(CancellationToken token);

        Task LogInterviewAsSuccessfullyHandledAsync(Guid interviewId);

        Task<List<CommittedEvent>> GetInterviewDetailsAsync(Guid interviewId, IProgress<TransferProgress> transferProgress, CancellationToken token);

        Task UploadInterviewAsync(Guid interviewId, InterviewPackageApiView completedInterview, IProgress<TransferProgress> transferProgress, CancellationToken token);
        Task UploadInterviewImageAsync(Guid interviewId, string fileName, byte[] fileData, IProgress<TransferProgress> transferProgress, CancellationToken token);
        Task UploadInterviewAudioAsync(Guid interviewId, string fileName, string contentType, byte[] fileData, IProgress<TransferProgress> transferProgressd, CancellationToken token);
        Task<List<string>> GetAttachmentContentsAsync(QuestionnaireIdentity questionnaire, IProgress<TransferProgress> transferProgress, CancellationToken token);
        Task<AttachmentContent> GetAttachmentContentAsync(string contentId, IProgress<TransferProgress> transferProgress, CancellationToken token);
        Task<List<QuestionnaireIdentity>> GetServerQuestionnairesAsync(CancellationToken cancellationToken);
        Task<List<TranslationDto>> GetQuestionnaireTranslationAsync(QuestionnaireIdentity questionnaireIdentity, CancellationToken cancellationToken);

        Task<CompanyLogoInfo> GetCompanyLogo(string storedClientEtag, CancellationToken cancellationToken);
        Task SendSyncStatisticsAsync(SyncStatisticsApiView statistics, CancellationToken token, RestCredentials credentials);
        Task SendUnexpectedExceptionAsync(UnexpectedExceptionApiView exception, CancellationToken token);
        
        Task<List<MapView>> GetMapList(CancellationToken cancellationToken);
        Task<RestStreamResult> GetMapContentStream(string mapName, CancellationToken cancellationToken);
        Task<Guid> GetCurrentSupervisor(CancellationToken token, RestCredentials credentials);

        Task<bool> IsAutoUpdateEnabledAsync(CancellationToken token);
        Task UploadAuditLogEntityAsync(AuditLogEntitiesApiView auditLogEntity, CancellationToken cancellationToken);
        Task<List<Guid>> CheckObsoleteInterviewsAsync(List<ObsoletePackageCheck> checks, CancellationToken cancellationToken);
        Task<AssignmentApiDocument> GetAssignmentAsync(int id, CancellationToken cancellationToken);
        Task<List<AssignmentApiView>> GetAssignmentsAsync(CancellationToken cancellationToken);
        Task LogAssignmentAsHandledAsync(int id, CancellationToken cancellationToken);
        Task SendSyncCompletedAsync(CancellationToken cancellationToken);
    }
}
