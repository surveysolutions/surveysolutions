using WB.Core.SharedKernels.Enumerator.Utils;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public interface ICompositeQuestionWithChildren : ICompositeQuestion
    {
        IObserbableCollection<ICompositeEntity> Children { get; }
    }
}