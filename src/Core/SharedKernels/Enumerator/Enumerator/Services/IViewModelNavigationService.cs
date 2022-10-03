using System.Threading.Tasks;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IViewModelNavigationService
    {
        Task<bool> NavigateToAsync<TViewModel, TParam>(TParam param) where TViewModel : IMvxViewModel<TParam>;
        Task NavigateToAsync<TViewModel>() where TViewModel : IMvxViewModel;
        Task<bool> NavigateToDashboardAsync(string interviewId = null);
        void NavigateToSettings();
        Task SignOutAndNavigateToLoginAsync();
        Task NavigateToLoginAsync();
        Task NavigateToFinishInstallationAsync();
        Task NavigateToMapsAsync();
        Task<bool> NavigateToInterviewAsync(string interviewId, NavigationIdentity navigationIdentity);
        Task NavigateToPrefilledQuestionsAsync(string interviewId);
        void NavigateToSplashScreen();
        void ShowWaitMessage();
        bool HasPendingOperations { get; }
        Task Close(IMvxViewModel viewModel);
        void InstallNewApp(string pathToApk);
        void CloseApplication();

        Task NavigateToCreateAndLoadInterview(int assignmentId);
        void NavigateToSystemDateSettings();
    }
}
