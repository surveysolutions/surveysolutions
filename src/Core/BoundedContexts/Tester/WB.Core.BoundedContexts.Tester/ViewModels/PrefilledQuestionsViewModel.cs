using WB.Core.BoundedContexts.Tester.Implementation.Entities;
using WB.Core.BoundedContexts.Tester.Repositories;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Tester.ViewModels
{
    public class PrefilledQuestionsViewModel : EnumeratorPrefilledQuestionsViewModel
    {
        public PrefilledQuestionsViewModel(IInterviewViewModelFactory interviewViewModelFactory,
            IPlainKeyValueStorage<QuestionnaireModel> plainQuestionnaireRepository,
            IStatefulInterviewRepository interviewRepository, 
            IViewModelNavigationService viewModelNavigationService) : 
            base(interviewViewModelFactory, plainQuestionnaireRepository, interviewRepository, viewModelNavigationService)
        {
        }

        protected override void NavigateToInterview()
        {
            this.viewModelNavigationService.NavigateTo<InterviewViewModel>(new { interviewId = this.interviewId });
        }

        protected override void AfterInterviewItemsAdded()
        {
            var startButton = this.interviewViewModelFactory.GetNew<StartInterviewViewModel>();
            startButton.Init(interviewId, null, null);
            this.PrefilledQuestions.Add(startButton);
        }

        public override void NavigateToPreviousViewModel()
        {
            this.viewModelNavigationService.NavigateTo<DashboardViewModel>();
        }
    }
}