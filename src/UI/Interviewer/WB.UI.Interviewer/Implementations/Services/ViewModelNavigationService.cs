using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.UI.Interviewer.ViewModel;

namespace WB.UI.Interviewer.Implementations.Services
{
    internal class ViewModelNavigationService : BaseViewModelNavigationService, IViewModelNavigationService
    {
        public ViewModelNavigationService(ICommandService commandService,
            IUserInteractionService userInteractionService,
            IUserInterfaceStateService userInterfaceStateService)
            : base(commandService, userInteractionService, userInterfaceStateService)
        {

        }
        public async Task NavigateToAsync<TViewModel>() where TViewModel : IMvxViewModel => await this.NavigateToAsync<TViewModel>(null);

        public async Task NavigateToDashboardAsync() => await this.NavigateToAsync<DashboardViewModel>();
        public async Task NavigateToLoginAsync() => await this.NavigateToAsync<LoginViewModel>();
        public async Task NavigateToInterviewAsync(string interviewId) => await this.NavigateToAsync<InterviewerInterviewViewModel>(new { interviewId = interviewId });
        public async Task NavigateToPrefilledQuestionsAsync(string interviewId) => await this.NavigateToAsync<PrefilledQuestionsViewModel>(new { interviewId = interviewId });
    }
}