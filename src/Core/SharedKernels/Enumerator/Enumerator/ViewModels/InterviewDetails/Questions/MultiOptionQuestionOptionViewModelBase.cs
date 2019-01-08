using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;

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
            get => this.title;
            set => this.RaiseAndSetIfChanged(ref this.title, value);
        }

        public bool Checked
        {
            get => this.@checked;
            set => this.SetProperty(ref this.@checked, value);
        }

        public int? CheckedOrder
        {
            get => this.checkedOrder;
            set => this.SetProperty(ref this.checkedOrder, value);
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

        public bool IsRosterSize { get; set; }

        public IMvxAsyncCommand CheckAnswerCommand => new MvxAsyncCommand(CheckAnswerAsync, () => CanBeChecked && !IsProtected);

        protected virtual async Task CheckAnswerAsync()
        {
            this.SortCheckedOptions();
            await this.QuestionViewModel.ToggleAnswerAsync(this);
        }

        protected void SortCheckedOptions()
        {
            if (!QuestionViewModel.AreAnswersOrdered) return;

            if (this.Checked)
            {
                this.CheckedOrder = this.QuestionViewModel.Options
                                        .Select(x => x.CheckedOrder ?? 0)
                                        .DefaultIfEmpty(0)
                                        .Max() + 1;
            }
            else
            {
                if (this.CheckedOrder.HasValue)
                {
                    this.QuestionViewModel.Options
                        .Where(x => x.CheckedOrder > this.CheckedOrder)
                        .ForEach(x => x.CheckedOrder -= 1);
                }

                this.CheckedOrder = null;
            }
        }
    }
}
