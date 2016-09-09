using System;
using MvvmCross.Core.ViewModels;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class MultiOptionLinkedQuestionOptionViewModel : MvxNotifyPropertyChanged,
        ICompositeEntity
    {
        private readonly MultiOptionLinkedQuestionViewModel questionViewModel;

        private string title;
        private bool @checked;

        public MultiOptionLinkedQuestionOptionViewModel(MultiOptionLinkedQuestionViewModel questionViewModel)
        {
            this.questionViewModel = questionViewModel;
        }

        public string Title
        {
            get { return this.title; }
            set { this.title = value; this.RaisePropertyChanged(); }
        }

        public decimal[] Value { get; set; }

        public DateTime CheckedTimeStamp { get; private set; }

        private int? checkedOrder;

        public int? CheckedOrder
        {
            get { return this.checkedOrder; }
            set
            {
                this.checkedOrder = value;
                this.RaisePropertyChanged();
            }
        }

        public bool Checked
        {
            get { return this.@checked; }
            set
            {
                this.@checked = value;
                this.CheckedTimeStamp = value ? DateTime.Now : DateTime.MinValue;

                this.RaisePropertyChanged();
            }
        }

        public IMvxCommand CheckAnswerCommand
        {
            get
            {
                return new MvxCommand(async () => await this.questionViewModel.ToggleAnswerAsync(this));
            }
        }

        public QuestionStateViewModel<MultipleOptionsLinkedQuestionAnswered> QuestionState { get; set; }
    }
}