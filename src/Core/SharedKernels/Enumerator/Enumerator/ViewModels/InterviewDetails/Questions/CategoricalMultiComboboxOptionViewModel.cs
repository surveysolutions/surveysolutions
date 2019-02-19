using MvvmCross.Commands;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class CategoricalMultiComboboxOptionViewModel : CategoricalMultiOptionViewModel
    {
        public CategoricalMultiComboboxOptionViewModel(IUserInteractionService userInteraction) : base(userInteraction)
        {
        }

        public IMvxCommand RemoveAnswerCommand => new MvxCommand(() =>
        {
            this.Checked = false;
            this.CheckAnswerCommand.Execute();

        }, () => CanBeChecked && !IsProtected);
    }
}
