using System.Collections;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.Tester.Services
{
    public interface IInterviewViewModelFactory
    {
        IList GetEntities(string interviewId, Identity groupIdentity, NavigationState navigationState);
        IList GetPrefilledQuestions(string interviewId);
        T GetNew<T>() where T : class;
    }
}