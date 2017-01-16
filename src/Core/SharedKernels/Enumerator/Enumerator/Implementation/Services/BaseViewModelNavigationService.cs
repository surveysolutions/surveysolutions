using MvvmCross.Core.ViewModels;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public abstract class BaseViewModelNavigationService : MvxNavigatingObject
    {
        private readonly ICommandService commandService;
        private readonly IUserInteractionService userInteractionService;
        private readonly IUserInterfaceStateService userInterfaceStateService;
        private readonly IPrincipal principal;

        protected BaseViewModelNavigationService(ICommandService commandService,
            IUserInteractionService userInteractionService,
            IUserInterfaceStateService userInterfaceStateService,
            IPrincipal principal)
        {
            this.commandService = commandService;
            this.userInteractionService = userInteractionService;
            this.userInterfaceStateService = userInterfaceStateService;
            this.principal = principal;
        }

        public virtual bool HasPendingOperations => this.commandService.HasPendingCommands ||
                                                    this.userInteractionService.HasPendingUserInterations ||
                                                    this.userInterfaceStateService.IsUserInferfaceLocked;

        public abstract void NavigateToLogin();
        protected abstract void FinishActivity();

        protected abstract void NavigateToSettingsImpl();

        public virtual void NavigateTo<TViewModel>(object parameters) where TViewModel : IMvxViewModel
        {
            if (this.HasPendingOperations)
                this.ShowWaitMessage();
            else
                this.ShowViewModel<TViewModel>(parameters);
        }

        public void SignOutAndNavigateToLogin()
        {
            if (this.HasPendingOperations)
                this.ShowWaitMessage();
            else
            {
                this.principal.SignOut();
                this.NavigateToLogin();
                this.FinishActivity();
            }
        }

        public virtual void NavigateToSettings()
        {
            if (this.HasPendingOperations)
                this.ShowWaitMessage();
            else
                this.NavigateToSettingsImpl();
        }

        public void ShowWaitMessage()
            => this.InvokeOnMainThread(() => this.userInteractionService.ShowToast(UIResources.Messages_WaitPendingOperation));
    }
}