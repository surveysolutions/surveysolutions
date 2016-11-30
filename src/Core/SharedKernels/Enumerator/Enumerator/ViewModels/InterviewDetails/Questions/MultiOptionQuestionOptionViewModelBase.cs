using System;
using MvvmCross.Core.ViewModels;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class MultiOptionQuestionOptionViewModelBase : MvxNotifyPropertyChanged, ICompositeEntity
    {
        private string title;
        private bool @checked;
        private int? checkedOrder;

        public MultiOptionQuestionOptionViewModelBase(IMultiOptionQuestionViewModelToggleable questionViewModel)
        {
            this.QuestionViewModel = questionViewModel;
        }

        public IMultiOptionQuestionViewModelToggleable QuestionViewModel { get; private set; }

        public string Title
        {
            get { return this.title; }
            set { this.RaiseAndSetIfChanged(ref this.title, value); }
        }

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
    }
}