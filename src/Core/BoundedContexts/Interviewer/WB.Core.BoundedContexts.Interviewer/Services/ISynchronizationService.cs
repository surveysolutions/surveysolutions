using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.WebApi;

namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public interface ISynchronizationService
    {
        Task<InterviewerApiView> GetCurrentInterviewerAsync(string login, string password);
        Task<bool> HasCurrentInterviewerDeviceAsync();

        Task<bool> IsDeviceLinkedToCurrentInterviewerAsync();
        Task LinkCurrentInterviewerToDeviceAsync();

        Task<byte[]> GetQuestionnaireAssemblyAsync(QuestionnaireIdentity questionnaire, Action<decimal, long, long> onDownloadProgressChanged, CancellationToken token);
        Task<QuestionnaireApiView> GetQuestionnaireAsync(QuestionnaireIdentity questionnaire, Action<decimal, long, long> onDownloadProgressChanged, CancellationToken token);
        Task<List<QuestionnaireIdentity>> GetCensusQuestionnairesAsync();

        Task<byte[]> GetApplicationAsync(CancellationToken token);
        Task<int?> GetLatestApplicationVersionAsync();
        Task CheckInterviewerCompatibilityWithServerAsync();
        Task SendTabletInformationAsync(string archive);

        Task<List<InterviewApiView>> GetInterviewsAsync();
        Task<List<SynchronizationChunkMeta>> GetInterviewPackagesAsync(string lastPackageId);
        Task LogPackageAsSuccessfullyHandledAsync(string packageId);

        Task<InterviewSyncPackageDto> GetInterviewPackageAsync(string packageId, string previousSuccessfullyHandledPackageId, Action<decimal, long, long> onDownloadProgressChanged, CancellationToken token);
        Task UploadInterviewAsync(Guid interviewId, string content, Action<decimal, long, long> onDownloadProgressChanged, CancellationToken token);
        Task UploadInterviewImageAsync(Guid interviewId, string fileName, byte[] fileData, Action<decimal, long, long> onDownloadProgressChanged, CancellationToken token);
    }
}
