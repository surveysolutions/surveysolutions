using System.Collections.Generic;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IInterviewViewModelFactory
    {
        Task<IEnumerable<IInterviewEntityViewModel>> GetEntities(string interviewId, Identity groupIdentity, NavigationState navigationState);
        Task<IEnumerable<IInterviewEntityViewModel>> GetPrefilledQuestions(string interviewId);
        T GetNew<T>() where T : class;
    }
}