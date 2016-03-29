using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public interface IInterviewEntityViewModel
    {
        Identity Identity { get; }
        Task InitAsync(string interviewId, Identity entityIdentity, NavigationState navigationState);
    }
}