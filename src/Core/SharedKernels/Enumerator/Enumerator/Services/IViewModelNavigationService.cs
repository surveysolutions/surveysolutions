using System.Threading.Tasks;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IViewModelNavigationService
    {
        Task NavigateToAsync<TViewModel, TParam>(TParam param) where TViewModel : IMvxViewModel<TParam>;
        Task NavigateToAsync<TViewModel>() where TViewModel : IMvxViewModel;
        Task NavigateToDashboardAsync(string interviewId = null);
        void NavigateToSettings();
        Task SignOutAndNavigateToLoginAsync();
        Task NavigateToLoginAsync();
        Task NavigateToFinishInstallationAsync();
        Task NavigateToMapsAsync();
        Task NavigateToInterviewAsync(string interviewId, NavigationIdentity navigationIdentity);
        Task NavigateToPrefilledQuestionsAsync(string interviewId);
        void NavigateToSplashScreen();
        void ShowWaitMessage();
        bool HasPendingOperations { get; }
        Task Close(IMvxViewModel viewModel);
    }
}
