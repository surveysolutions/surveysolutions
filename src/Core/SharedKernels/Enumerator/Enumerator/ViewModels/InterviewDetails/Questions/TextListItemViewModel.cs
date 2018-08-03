using System;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class TextListItemViewModel : MvxNotifyPropertyChanged, ICompositeEntity
    {
        private readonly QuestionStateViewModel<TextListQuestionAnswered> questionState;
        public IQuestionStateViewModel QuestionState => this.questionState;

        public TextListItemViewModel(QuestionStateViewModel<TextListQuestionAnswered> questionState)
        {
            this.questionState = questionState;
        }


        public event EventHandler ItemEdited;
        public event EventHandler ItemDeleted;

        public decimal Value { get; set; }

        private string title;
        private bool isProtected;

        public string Title
        {
            get => this.title;
            set
            {
                this.title = value;
                this.RaisePropertyChanged();
            }
        }
        
        public IMvxCommand ValueChangeCommand => new MvxCommand(this.OnItemEdited);

        public IMvxCommand DeleteListItemCommand => new MvxCommand(this.DeleteListItem);

        private void DeleteListItem() => this.ItemDeleted?.Invoke(this, EventArgs.Empty);

        private void OnItemEdited() => this.ItemEdited?.Invoke(this, EventArgs.Empty);

        public string ItemTag => this.QuestionState.Header.Identity + "_Item_" + Value;

        public bool IsProtected
        {
            get => isProtected;
            set
            {
                if (value == isProtected) return;
                isProtected = value;
                RaisePropertyChanged(() => IsProtected);
            }
        }
    }
}
