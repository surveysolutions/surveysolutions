using System;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.UI.Shared.Enumerator.Activities
{
    public class AppStart : MvxAppStart
    {
        private readonly IPrincipal principal;
        private readonly IViewModelNavigationService navigationService;

        public AppStart(IMvxApplication application, IPrincipal principal, IViewModelNavigationService navigationService) : base(application)
        {
            this.principal = principal ?? throw new ArgumentNullException(nameof(principal));
            this.navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        }

        protected override void ApplicationStartup(object hint = null)
        {
            base.Startup(hint);

            if (principal.IsAuthenticated)
            {
                this.navigationService.NavigateToDashboardAsync().WaitAndUnwrapException();
            }
            else
            {
                this.navigationService.NavigateToLoginAsync().WaitAndUnwrapException();
            }
        }
    }
}
