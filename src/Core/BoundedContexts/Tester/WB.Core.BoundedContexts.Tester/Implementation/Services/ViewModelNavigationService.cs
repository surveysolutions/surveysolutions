using Cirrious.MvvmCross.ViewModels;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Tester.Implementation.Services
{
    public class ViewModelNavigationService : MvxNavigatingObject, IViewModelNavigationService
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
            this.NavigateTo<DashboardViewModel>();
        }

        public void NavigateToInterview(string interviewId)
        {
            this.NavigateTo<InterviewViewModel>(new {interviewId = interviewId});
        }

        public void NavigateToPrefilledQuestions(string interviewId)
        {
            this.NavigateTo<PrefilledQuestionsViewModel>(new { interviewId = interviewId });
        }
    }
}