using Cheesebaron.MvxPlugins.Settings.Interfaces;
using IHS.MvvmCross.Plugins.Keychain;
using WB.Infrastructure.Shared.Enumerator.Internals.Security;

namespace WB.UI.Capi.Infrastructure.Internals
{
    public class InterviewerPrincipal : Principal
    {
        public InterviewerPrincipal(IKeychain securityService,
            ISettings settingsService) : base(securityService, settingsService)
        {
        }


    }
}