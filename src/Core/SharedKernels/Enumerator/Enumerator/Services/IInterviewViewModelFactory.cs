using System.Collections.Generic;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IInterviewViewModelFactory
    {
        IEnumerable<Task<IInterviewEntityViewModel>> GetEntities(string interviewId, Identity groupIdentity, NavigationState navigationState);
        IEnumerable<Task<IInterviewEntityViewModel>> GetPrefilledQuestions(string interviewId);
        T GetNew<T>() where T : class;
    }
}