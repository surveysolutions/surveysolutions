using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public interface IInterviewEntity
    {
        void Init(string interviewId, Identity identity);
    }
}