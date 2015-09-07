using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
        Task<IEnumerable<QuestionnaireIdentity>> GetCensusQuestionnairesAsync();

        Task<byte[]> GetApplicationAsync(CancellationToken token);
        Task<int?> GetLatestApplicationVersionAsync();
        Task CheckInterviewerCompatibilityWithServerAsync();
        Task SendTabletInformationAsync(string archive);

        Task<IEnumerable<InterviewApiView>> GetInterviewsAsync();
    }
}
