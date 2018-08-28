using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Utils;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public interface IMultiOptionQuestionViewModelToggleable
    {
        Task ToggleAnswerAsync(MultiOptionQuestionOptionViewModelBase changedModel);

        IObservableCollection<MultiOptionQuestionOptionViewModelBase> Options { get; }

        Identity QuestionIdentity { get; }

        bool AreAnswersOrdered { get; }
    }
}
