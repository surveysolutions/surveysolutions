using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class MultiOptionQuestionOptionViewModel : MultiOptionQuestionOptionViewModelBase
    {
        public MultiOptionQuestionOptionViewModel(IMultiOptionQuestionViewModelToggleable questionViewModel) : base(questionViewModel)
        {
        }

        public int Value { get; set; }

        public QuestionStateViewModel<MultipleOptionsQuestionAnswered> QuestionState { get; set; }
    }
}