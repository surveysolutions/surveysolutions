using System;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;


namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class YesNoQuestionOptionViewModel : MvxNotifyPropertyChanged
    {
        public YesNoQuestionViewModel QuestionViewModel { get; private set; }
        public QuestionStateViewModel<YesNoQuestionAnswered> QuestionState { get; set; }

        public YesNoQuestionOptionViewModel(YesNoQuestionViewModel questionViewModel,
            QuestionStateViewModel<YesNoQuestionAnswered> questionState)
        {
            this.QuestionViewModel = questionViewModel;
            this.QuestionState = questionState;
        }

        public decimal Value { get; set; }

        private string title;
        public string Title
        {
            get { return this.title; }
            set { this.title = value; this.RaisePropertyChanged(); }
        }

        private bool? selected;

        public bool? Selected
        {
            get { return this.selected; }
            set
            {
                if (this.selected == value)
                    return;

                this.selected = value;
                this.CheckedTimeStamp = value.HasValue ? DateTime.Now : DateTime.MinValue;

                this.RaiseAnswerCommand.Execute();

                this.RaisePropertyChanged();
                this.RaisePropertyChanged(() => YesSelected);
                this.RaisePropertyChanged(() => NoSelected);
            }
        }

        public bool YesSelected
        {
            get { return this.Selected.HasValue && this.Selected.Value; }
            set { this.Selected = true; }
        }

        public bool NoSelected
        {
            get { return this.Selected.HasValue && !this.Selected.Value; }
            set { this.Selected = false; }
        }

        private int? checkedOrder;

        public int? CheckedOrder
        {
            get { return this.checkedOrder; }
            set
            {
                if (this.checkedOrder == value)
                    return;

                this.checkedOrder = value;
                this.RaisePropertyChanged();
            }
        }

        public DateTime CheckedTimeStamp { get; private set; }

        public IMvxCommand RaiseAnswerCommand
        {
            get { return new MvxCommand(async () => await this.QuestionViewModel.ToggleAnswerAsync(this)); }
        }

        public IMvxCommand RemoveAnswerCommand
        {
            get { return new MvxCommand(() => { Selected = null; }); }
        }
    }
}