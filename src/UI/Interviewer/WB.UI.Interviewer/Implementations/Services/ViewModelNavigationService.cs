using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.UI.Interviewer.ViewModel;

namespace WB.UI.Interviewer.Implementations.Services
{
    internal class ViewModelNavigationService : MvxNavigatingObject, IViewModelNavigationService
    {
        public void NavigateTo<TViewModel>() where TViewModel : IMvxViewModel
        {
            this.ShowViewModel<TViewModel>();
        }

        public void NavigateTo<TViewModel>(object perameterValuesObject) where TViewModel : IMvxViewModel
        {
            this.ShowViewModel<TViewModel>(perameterValuesObject);
        }

        public void NavigateToDashboard()
        {
             this.NavigateTo<DashboardViewModel>(new { });
        }

        public void NavigateToPrefilledQuestions(string interviewId)
        {
            this.NavigateTo<PrefilledQuestionsViewModel>(new { interviewId = interviewId });
        }

        public void NavigateToInterview(string interviewId)
        {
            this.NavigateTo<InterviewerInterviewViewModel>(new { interviewId = interviewId });
        }
    }
}