using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;

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