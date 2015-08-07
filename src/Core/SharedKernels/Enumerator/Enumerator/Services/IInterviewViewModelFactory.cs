using System.Collections;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.BoundedContexts.Tester.ViewModels.InterviewEntities;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.Tester.Services
{
    public interface IInterviewViewModelFactory
    {
        IEnumerable<IInterviewEntityViewModel> GetEntities(string interviewId, Identity groupIdentity, NavigationState navigationState);
        IEnumerable<IInterviewEntityViewModel> GetPrefilledQuestions(string interviewId);
        IEnumerable<dynamic> GetCompleteScreenEntities(string interviewId);
        T GetNew<T>() where T : class;
    }
}