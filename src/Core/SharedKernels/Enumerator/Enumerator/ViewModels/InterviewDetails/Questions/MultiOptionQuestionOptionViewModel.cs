using System;
using MvvmCross.Core.ViewModels;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class MultiOptionQuestionOptionViewModel : MvxNotifyPropertyChanged, ICompositeEntity
    {
        public MultiOptionQuestionViewModel QuestionViewModel { get; private set; }

        public MultiOptionQuestionOptionViewModel(MultiOptionQuestionViewModel questionViewModel)
        {
            this.QuestionViewModel = questionViewModel;
        }

        public int Value { get; set; }

        private string title;
        public string Title
        {
            get { return this.title; }
            set { this.title = value; this.RaisePropertyChanged(); }
        }

        private bool @checked;

        public bool Checked
        {
            get { return this.@checked; }
            set
            {
                if (this.@checked == value) return;
                this.@checked = value;
                this.CheckedTimeStamp = value ? DateTime.Now : DateTime.MinValue;

                this.RaisePropertyChanged();
            }
        }

        private int? checkedOrder;

        public int? CheckedOrder
        {
            get { return this.checkedOrder; }
            set
            {
                if (this.checkedOrder == value) return;
                this.checkedOrder = value; 
                this.RaisePropertyChanged();
            }
        }

        public DateTime CheckedTimeStamp { get; private set; }

        public IMvxCommand CheckAnswerCommand
        {
            get
            {
                return new MvxCommand(async () => await this.QuestionViewModel.ToggleAnswerAsync(this));
            }
        }

        public QuestionStateViewModel<MultipleOptionsQuestionAnswered> QuestionState { get; set; }
    }
}