using Android.Content;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Droid.Platform;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.UI.Capi.ViewModel
{
    public class InterviewerPrefilledQuestionsViewModel : EnumeratorPrefilledQuestionsViewModel
    {
        public InterviewerPrefilledQuestionsViewModel(IInterviewViewModelFactory interviewViewModelFactory,
            IPlainKeyValueStorage<QuestionnaireModel> plainQuestionnaireRepository,
            IStatefulInterviewRepository interviewRepository, 
            IViewModelNavigationService viewModelNavigationService) : 
            base(interviewViewModelFactory, plainQuestionnaireRepository, interviewRepository, viewModelNavigationService)
        {
        }

        protected override void NavigateToInterview()
        {
            this.viewModelNavigationService.NavigateTo<InterviewerInterviewViewModel>(new { interviewId = this.interviewId });
        }

        public override void NavigateToPreviousViewModel()
        {
            viewModelNavigationService.NavigateToDashboard();
        }

        protected override void AfterInterviewItemsAdded()
        {
            var startButton = this.interviewViewModelFactory.GetNew<StartInterviewViewModel>();
            startButton.Init(interviewId, null, null);
            this.PrefilledQuestions.Add(startButton);
        }
    }
}