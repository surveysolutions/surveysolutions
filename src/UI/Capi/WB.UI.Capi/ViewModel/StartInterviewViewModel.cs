using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.Infrastructure.CommandBus;

namespace WB.UI.Capi.ViewModel
{
    public class StartInterviewViewModel : EnumeratorStartInterviewViewModel
    {
        readonly IViewModelNavigationService viewModelNavigationService;

        public StartInterviewViewModel(ICommandService commandService, IViewModelNavigationService viewModelNavigationService) : base(commandService)
        {
            this.viewModelNavigationService = viewModelNavigationService;
        }

        protected override void NavigateToInterview()
        {
            this.viewModelNavigationService.NavigateTo<InterviewerInterviewViewModel>(new { interviewId = this.interviewId });
        }
    }
}