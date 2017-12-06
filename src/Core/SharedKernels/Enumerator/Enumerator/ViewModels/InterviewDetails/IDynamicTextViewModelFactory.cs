using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public interface IDynamicTextViewModelFactory
    {
        DynamicTextViewModel CreateDynamicTextViewModel();
        ErrorMessageViewModel CreateErrorMessage();
    }
}