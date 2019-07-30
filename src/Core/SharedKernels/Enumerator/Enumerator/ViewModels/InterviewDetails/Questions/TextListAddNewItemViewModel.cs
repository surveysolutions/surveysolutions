using System.Windows.Input;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class TextListAddNewItemViewModel : MvxNotifyPropertyChanged, ICompositeEntity
    {
        private readonly QuestionStateViewModel<TextListQuestionAnswered> questionState;
        public IQuestionStateViewModel QuestionState => this.questionState;

        public TextListAddNewItemViewModel(QuestionStateViewModel<TextListQuestionAnswered> questionState,
            IMvxAsyncCommand<string> addListItem)
        {
            this.questionState = questionState;
            this.AddNewItemCommand = addListItem;
        }
        
        private string text;
        public string Text
        {
            get => this.text;
            set
            {
                this.text = value;
                this.RaisePropertyChanged();
            }
        }
        
        public IMvxAsyncCommand<string> AddNewItemCommand { get; }
    }
}
