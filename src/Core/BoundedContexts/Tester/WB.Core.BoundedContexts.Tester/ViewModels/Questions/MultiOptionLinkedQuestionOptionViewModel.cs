using System;
using Cirrious.MvvmCross.ViewModels;

namespace WB.Core.BoundedContexts.Tester.ViewModels.Questions
{
    public class MultiOptionLinkedQuestionOptionViewModel : MvxNotifyPropertyChanged
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
    }
}