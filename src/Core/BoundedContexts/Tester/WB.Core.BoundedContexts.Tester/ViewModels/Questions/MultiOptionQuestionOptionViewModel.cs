using System;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Tester.ViewModels.Questions.State;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Core.BoundedContexts.Tester.ViewModels.Questions
{
    public class MultiOptionQuestionOptionViewModel : MvxNotifyPropertyChanged
    {
        public MultiOptionQuestionViewModel QuestionViewModel { get; private set; }

        public MultiOptionQuestionOptionViewModel(MultiOptionQuestionViewModel questionViewModel)
        {
            this.QuestionViewModel = questionViewModel;
        }

        public decimal Value { get; set; }

        private string title;
        public string Title
        {
            get { return this.title; }
            set { this.title = value; this.RaisePropertyChanged(); }
        }

        private bool @checked;

        public bool Checked
        {
            get { return @checked; }
            set
            {
                @checked = value;
                if (value)
                {
                    this.CheckedTimeStamp = DateTime.Now;
                }
                else
                {
                    this.CheckedTimeStamp = DateTime.MinValue;
                }

                RaisePropertyChanged();
            }
        }

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

        public DateTime CheckedTimeStamp { get; private set; }

        public IMvxCommand CheckAnswerCommand
        {
            get
            {
                return new MvxCommand(async () => await QuestionViewModel.ToggleAnswer(this));
            }
        }

        public QuestionStateViewModel<MultipleOptionsQuestionAnswered> QuestionState { get; set; }
    }
}