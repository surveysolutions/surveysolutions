using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public interface IInterviewEntityViewModel
    {
        void Init(string interviewId, Identity entityIdentity, NavigationState navigationState);
    }
}