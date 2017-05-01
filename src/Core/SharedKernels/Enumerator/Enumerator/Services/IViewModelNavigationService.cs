using MvvmCross.Core.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IViewModelNavigationService
    {
        void NavigateTo<TViewModel>() where TViewModel : IMvxViewModel;
        void NavigateTo<TViewModel>(object parameters) where TViewModel : IMvxViewModel;
        void NavigateToDashboard();
        void NavigateToSettings();
        void SignOutAndNavigateToLogin();
        void NavigateToLogin();
        void NavigateToInterview(string interviewId, NavigationIdentity navigationIdentity);
        void NavigateToPrefilledQuestions(string interviewId);
        void ShowWaitMessage();
        bool HasPendingOperations { get; }
        void NavigateToAreaEditor(string geometry, string mapName, double? areaSize);
    }
}