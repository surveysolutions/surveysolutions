using System;
using System.Linq;
using MvvmCross.Core.ViewModels;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class MultiOptionQuestionOptionViewModelBase : MvxNotifyPropertyChanged, ICompositeEntity
    {
        private string title;
        private bool @checked;
        private bool canBeChecked = true;
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
                if (value)
                {
                    this.CheckedOrder = this.QuestionViewModel.Options.Max(x => x.CheckedOrder) + 1;
                }
                else
                {
                    this.CheckedOrder = null;
                }

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

        public bool CanBeChecked
        {
            get => this.canBeChecked;
            set => SetProperty(ref this.canBeChecked, value);
        }

        public IMvxCommand CheckAnswerCommand
        {
            get
            {
                return new MvxCommand(async () => await this.QuestionViewModel.ToggleAnswerAsync(this));
            }
        }
    }
}
