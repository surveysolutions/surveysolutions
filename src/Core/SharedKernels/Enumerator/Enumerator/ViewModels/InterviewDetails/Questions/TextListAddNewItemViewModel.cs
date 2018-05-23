using System;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class TextListItemAddedEventArgrs : EventArgs
    {
        public readonly string NewText;
        public   TextListItemAddedEventArgrs(string newText)
        {
            this.NewText = newText;
        }
    }
    public class TextListAddNewItemViewModel : MvxNotifyPropertyChanged, ICompositeEntity
    {
        private readonly QuestionStateViewModel<TextListQuestionAnswered> questionState;
        public IQuestionStateViewModel QuestionState => this.questionState;

        public TextListAddNewItemViewModel(QuestionStateViewModel<TextListQuestionAnswered> questionState)
        {
            this.questionState = questionState;
        }
        
        public event EventHandler<TextListItemAddedEventArgrs> ItemAdded;

        private string text;
        public string Text
        {
            get
            {
                return this.text;
            }
            set
            {
                this.text = value;
                this.RaisePropertyChanged();
            }
        }
        
        public IMvxCommand AddNewItemCommand => new MvxCommand(this.AddNewItem);

        private void AddNewItem()
        {
            if (string.IsNullOrWhiteSpace(this.Text)) return;

            this.ItemAdded?.Invoke(this, new TextListItemAddedEventArgrs(this.Text.Trim()));
        }
    }
}