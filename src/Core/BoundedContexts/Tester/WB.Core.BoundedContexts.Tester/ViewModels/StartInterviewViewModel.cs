using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.Infrastructure.CommandBus;

namespace WB.Core.BoundedContexts.Tester.ViewModels
{
    public class StartInterviewViewModel : EnumeratorStartInterviewViewModel
    {
        readonly IViewModelNavigationService viewModelNavigationService;

        protected override void NavigateToInterview()
        {
            this.viewModelNavigationService.NavigateTo<InterviewViewModel>(new { interviewId = this.interviewId });
        }

        public StartInterviewViewModel(ICommandService commandService, IViewModelNavigationService viewModelNavigationService) : base(commandService)
        {
            this.viewModelNavigationService = viewModelNavigationService;
        }
    }
}