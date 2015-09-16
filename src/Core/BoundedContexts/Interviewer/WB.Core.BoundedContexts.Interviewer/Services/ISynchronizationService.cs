using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.WebApi;

namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public interface ISynchronizationService
    {
        Task<InterviewerApiView> GetCurrentInterviewerAsync(string login, string password, CancellationToken token);
        Task<bool> HasCurrentInterviewerDeviceAsync(CancellationToken token, RestCredentials credentials = null);

        Task<bool> IsDeviceLinkedToCurrentInterviewerAsync(CancellationToken token, RestCredentials credentials = null);
        Task LinkCurrentInterviewerToDeviceAsync(CancellationToken token, RestCredentials credentials = null);

        Task<byte[]> GetQuestionnaireAssemblyAsync(QuestionnaireIdentity questionnaire, Action<decimal, long, long> onDownloadProgressChanged, CancellationToken token);
        Task<QuestionnaireApiView> GetQuestionnaireAsync(QuestionnaireIdentity questionnaire, Action<decimal, long, long> onDownloadProgressChanged, CancellationToken token);
        Task<List<QuestionnaireIdentity>> GetCensusQuestionnairesAsync(CancellationToken token);
        Task LogQuestionnaireAsSuccessfullyHandledAsync(QuestionnaireIdentity questionnaire, CancellationToken token);
        Task LogQuestionnaireAssemblyAsSuccessfullyHandledAsync(QuestionnaireIdentity questionnaire, CancellationToken token);

        Task<byte[]> GetApplicationAsync(CancellationToken token);
        Task<int?> GetLatestApplicationVersionAsync(CancellationToken token);
        Task CheckInterviewerCompatibilityWithServerAsync(CancellationToken token);
        Task SendTabletInformationAsync(string archive, CancellationToken token);

        Task<List<InterviewApiView>> GetInterviewsAsync(CancellationToken token);
        Task<InterviewPackagesApiView> GetInterviewPackagesAsync(string lastPackageId, CancellationToken token);

        Task LogPackageAsSuccessfullyHandledAsync(string packageId, CancellationToken token);

        Task<InterviewSyncPackageDto> GetInterviewPackageAsync(string packageId,string previousSuccessfullyHandledPackageId, Action<decimal, long, long> onDownloadProgressChanged, CancellationToken token);
        Task UploadInterviewAsync(Guid interviewId, string content, Action<decimal, long, long> onDownloadProgressChanged, CancellationToken token);
        Task UploadInterviewImageAsync(Guid interviewId, string fileName, byte[] fileData, Action<decimal, long, long> onDownloadProgressChanged, CancellationToken token);
    }
}
