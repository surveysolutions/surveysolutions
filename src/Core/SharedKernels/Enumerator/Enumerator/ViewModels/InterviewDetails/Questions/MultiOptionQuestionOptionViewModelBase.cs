using System.Linq;
using MvvmCross.Core.ViewModels;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class MultiOptionQuestionOptionViewModelBase : MvxNotifyPropertyChanged, ICompositeEntity
    {
        private string title;
        private bool @checked;
        private bool canBeChecked = true;
        private int? checkedOrder;
        private bool isProtected;

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

        public bool IsProtected
        {
            get => isProtected;
            set => SetProperty(ref isProtected, value);
        }

        public bool CanBeChecked
        {
            get => this.canBeChecked;
            set => SetProperty(ref this.canBeChecked, value);
        }

        public IMvxAsyncCommand CheckAnswerCommand
        {
            get
            {
                return new MvxAsyncCommand(async () => await this.QuestionViewModel.ToggleAnswerAsync(this), () => CanBeChecked);
            }
        }
    }
}
