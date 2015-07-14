using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.Tester.ViewModels.InterviewEntities
{
    public interface IInterviewEntityViewModel
    {
        void Init(string interviewId, Identity entityIdentity, NavigationState navigationState);
    }
}