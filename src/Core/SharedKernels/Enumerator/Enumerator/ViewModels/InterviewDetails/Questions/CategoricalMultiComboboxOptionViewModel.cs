using MvvmCross.Commands;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class CategoricalMultiComboboxOptionViewModel : CategoricalMultiOptionViewModel
    {
        public CategoricalMultiComboboxOptionViewModel(IUserInteractionService userInteraction, AttachmentViewModel attachmentViewModel)
            : base(userInteraction, attachmentViewModel)
        {
        }

        public IMvxCommand RemoveAnswerCommand => new MvxCommand(() =>
        {
            this.Checked = false;
            this.CheckAnswerCommand.Execute();

        }, () => CanBeChecked && !IsProtected);
    }
}
