using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public interface IInterviewEntity
    {
        string InterviewId { get; }
        Identity Identity { get; }
        NavigationState NavigationState { get; }
    }
}
