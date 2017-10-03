using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public interface ISynchronizationService
    {
        Task<string> LoginAsync(LogonInfo logonInfo, RestCredentials credentials, CancellationToken? token = null);
        Task<InterviewerApiView> GetInterviewerAsync(RestCredentials credentials = null, CancellationToken? token = null);
        Task<bool> HasCurrentInterviewerDeviceAsync(RestCredentials credentials = null, CancellationToken? token = null);

        Task CanSynchronizeAsync(RestCredentials credentials = null, CancellationToken? token = null);
        Task SendDeviceInfoAsync(DeviceInfoApiView info, CancellationToken? token = null);
        Task LinkCurrentInterviewerToDeviceAsync(RestCredentials credentials = null, CancellationToken? token = null);

        Task<byte[]> GetQuestionnaireAssemblyAsync(QuestionnaireIdentity questionnaire, Action<decimal, long, long> onDownloadProgressChanged, CancellationToken token);
        Task<QuestionnaireApiView> GetQuestionnaireAsync(QuestionnaireIdentity questionnaire, Action<decimal, long, long> onDownloadProgressChanged, CancellationToken token);
        Task<List<QuestionnaireIdentity>> GetCensusQuestionnairesAsync(CancellationToken token);
        Task LogQuestionnaireAsSuccessfullyHandledAsync(QuestionnaireIdentity questionnaire);
        Task LogQuestionnaireAssemblyAsSuccessfullyHandledAsync(QuestionnaireIdentity questionnaire);

        Task<byte[]> GetApplicationAsync(CancellationToken token, Action<DownloadProgressChangedEventArgs> onDownloadProgressChanged = null);
        Task<byte[]> GetApplicationPatchAsync(CancellationToken token, Action<DownloadProgressChangedEventArgs> onDownloadProgressChanged = null);
        Task<int?> GetLatestApplicationVersionAsync(CancellationToken token);
        Task SendBackupAsync(string filePath, CancellationToken token);

        Task<List<InterviewApiView>> GetInterviewsAsync(CancellationToken token);

        Task LogInterviewAsSuccessfullyHandledAsync(Guid interviewId);

        Task<InterviewerInterviewApiView> GetInterviewDetailsAsync(Guid interviewId, Action<decimal, long, long> onDownloadProgressChanged, CancellationToken token);
        Task UploadInterviewAsync(Guid interviewId, InterviewPackageApiView completedInterview, Action<decimal, long, long> onDownloadProgressChanged, CancellationToken token);
        Task UploadInterviewImageAsync(Guid interviewId, string fileName, byte[] fileData, Action<decimal, long, long> onDownloadProgressChanged, CancellationToken token);
        Task UploadInterviewAudioAsync(Guid interviewId, string fileName, string contentType, byte[] fileData, Action<decimal, long, long> onDownloadProgressChanged, CancellationToken token);
        Task<List<string>> GetAttachmentContentsAsync(QuestionnaireIdentity questionnaire, Action<decimal, long, long> onDownloadProgressChanged, CancellationToken token);
        Task<AttachmentContent> GetAttachmentContentAsync(string contentId, Action<decimal, long, long> onDownloadProgressChanged, CancellationToken token);
        Task<List<QuestionnaireIdentity>> GetServerQuestionnairesAsync(CancellationToken cancellationToken);
        Task<List<TranslationDto>> GetQuestionnaireTranslationAsync(QuestionnaireIdentity questionnaireIdentity, CancellationToken cancellationToken);

        Task<CompanyLogoInfo> GetCompanyLogo(string storedClientEtag, CancellationToken cancellationToken);
        Task SendSyncStatisticsAsync(SyncStatisticsApiView statistics, CancellationToken token, RestCredentials credentials);
        Task SendUnexpectedExceptionAsync(UnexpectedExceptionApiView exception, CancellationToken token);
        
        Task<List<MapView>> GetMapList(CancellationToken cancellationToken);
        Task<byte[]> GetMapContent(string url, CancellationToken cancellationToken);
    }
}
