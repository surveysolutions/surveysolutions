using System.Threading.Tasks;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public interface IMultiOptionQuestionViewModelToggleable
    {
        Task ToggleAnswerAsync(MultiOptionQuestionOptionViewModelBase changedModel);
    }
}