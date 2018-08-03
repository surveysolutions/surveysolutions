using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public interface IInterviewEntityViewModel : ICompositeEntity
    {
        Identity Identity { get; }
        void Init(string interviewId, Identity entityIdentity, NavigationState navigationState);
    }
}