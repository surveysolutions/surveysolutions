using System;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class BaseViewModelNavigationService : MvxNavigatingObject
    {
        private readonly ICommandService commandService;
        private readonly IUserInteractionService userInteractionService;
        private readonly IUserInterfaceStateService userInterfaceStateService;
        private readonly int waitDelay = 500;
        private readonly int maxNumberOfWaitAttemts = 10;

        public BaseViewModelNavigationService(ICommandService commandService,
            IUserInteractionService userInteractionService,
            IUserInterfaceStateService userInterfaceStateService)
        {
            this.commandService = commandService;
            this.userInteractionService = userInteractionService;
            this.userInterfaceStateService = userInterfaceStateService;
        }

        public virtual bool HasPendingOperations => this.commandService.HasPendingCommands ||
                                                    this.userInteractionService.HasPendingUserInterations ||
                                                    this.userInterfaceStateService.IsUserInferfaceLocked;

        public virtual async Task NavigateToAsync<TViewModel>(object parameters) where TViewModel : IMvxViewModel
        {
            await this.WaitPendingOperationsCompletionAsync();
            if (!this.HasPendingOperations)
                this.ShowViewModel<TViewModel>(parameters);
        }

        public async Task WaitPendingOperationsCompletionAsync()
        {
            int waitAttempts = 0;
            while (this.HasPendingOperations)
            {
                if(waitAttempts>this.maxNumberOfWaitAttemts)
                    return;

                this.userInteractionService.ShowToast(UIResources.Messages_WaitPendingOperation);
                await Task.Delay(waitDelay);
                waitAttempts++;
                
            }

            await this.userInteractionService.WaitPendingUserInteractionsAsync().ConfigureAwait(false);
            await this.userInterfaceStateService.WaitWhileUserInterfaceIsRefreshingAsync().ConfigureAwait(false);
            await this.commandService.WaitPendingCommandsAsync().ConfigureAwait(false);
        }
    }
}