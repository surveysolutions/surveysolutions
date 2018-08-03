using MvvmCross.ViewModels;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    internal static class BaseViewModelSetupMethods
    {
        public static void Prepare(bool isAuthenticationRequired, IPrincipal principal, IViewModelNavigationService navigationService)
        {
            if (isAuthenticationRequired && !principal.IsAuthenticated)
            {
                navigationService.NavigateToSplashScreen();
            }
        }

        public static void SaveStateToBundle(IPrincipal principal, IMvxBundle bundle)
        {
            if (principal?.IsAuthenticated ?? false)
            {
                bundle.Data["userName"] = principal.CurrentUserIdentity.Name;
                bundle.Data["passwordHash"] = principal.CurrentUserIdentity.PasswordHash;
            }
        }

        public static void ReloadStateFromBundle(IPrincipal principal, IMvxBundle bundle)
        {
            if (bundle.Data.ContainsKey("userName") && ! principal.IsAuthenticated)
            {
                principal.SignInWithHash(bundle.Data["userName"], bundle.Data["passwordHash"], true);
            }
        }
    }
}
