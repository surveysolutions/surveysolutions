using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IViewModelNavigationService
    {
        Task NavigateToAsync<TViewModel>() where TViewModel : IMvxViewModel;
        Task NavigateToAsync<TViewModel>(object parameters) where TViewModel : IMvxViewModel;
        Task NavigateToDashboardAsync();
        Task NavigateToLoginAsync();
        Task NavigateToInterviewAsync(string interviewId);
        Task NavigateToPrefilledQuestionsAsync(string interviewId);
        Task WaitPendingOperationsCompletionAsync();
        bool HasPendingOperations { get; }
    }
}