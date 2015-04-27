using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public interface IInterviewItemViewModel
    {
        void Init(string interviewId, Identity questionIdentity);
    }
}