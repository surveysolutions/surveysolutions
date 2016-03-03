using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Tester.Implementation.Services
{
    public class ViewModelNavigationService : MvxNavigatingObject, IViewModelNavigationService
    {
        private readonly ICommandService commandService;
        private readonly IUserInteractionService userInteractionServiceAwaiter;
        private readonly IUserInterfaceStateService userInterfaceStateService;

        public ViewModelNavigationService(ICommandService commandService,
            IUserInteractionService userInteractionServiceAwaiter,
            IUserInterfaceStateService userInterfaceStateService)
        {
            this.commandService = commandService;
            this.userInteractionServiceAwaiter = userInteractionServiceAwaiter;
            this.userInterfaceStateService = userInterfaceStateService;
        }

        public async Task NavigateToAsync<TViewModel>() where TViewModel : IMvxViewModel
        {
            await this.WaitPendingOperationsCompletionAsync();
            
            this.ShowViewModel<TViewModel>();
        }

        public async Task NavigateToAsync<TViewModel>(object perameterValuesObject) where TViewModel : IMvxViewModel
        {
            await this.WaitPendingOperationsCompletionAsync();
            
            this.ShowViewModel<TViewModel>(perameterValuesObject);
        }

        public async Task NavigateToDashboardAsync()
        {
            await this.NavigateToAsync<DashboardViewModel>();
        }

        public async Task NavigateToInterviewAsync(string interviewId)
        {
            await this.NavigateToAsync<InterviewViewModel>(new { interviewId = interviewId });
        }

        public async Task NavigateToPrefilledQuestionsAsync(string interviewId)
        {
            await this.NavigateToAsync<PrefilledQuestionsViewModel>(new { interviewId = interviewId });
        }

        private async Task WaitPendingOperationsCompletionAsync()
        {
            await this.userInteractionServiceAwaiter.WaitPendingUserInteractionsAsync().ConfigureAwait(false);
            await this.userInterfaceStateService.WaitWhileUserInterfaceIsRefreshingAsync().ConfigureAwait(false);
            await this.commandService.WaitPendingCommandsAsync().ConfigureAwait(false);
        }
    }
}