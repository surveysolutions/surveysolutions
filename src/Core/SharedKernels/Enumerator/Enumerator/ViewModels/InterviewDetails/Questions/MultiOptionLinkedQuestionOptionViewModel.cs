using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class MultiOptionLinkedQuestionOptionViewModel : MultiOptionQuestionOptionViewModelBase
    {
        public MultiOptionLinkedQuestionOptionViewModel(IMultiOptionQuestionViewModelToggleable questionViewModel) : base(questionViewModel)
        {
        }

        public decimal[] Value { get; set; }

        public QuestionStateViewModel<MultipleOptionsLinkedQuestionAnswered> QuestionState { get; set; }
    }
}