using System;
using HockeyApp.Android;
using MvvmCross;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.UI.Shared.Enumerator.Services
{
    public class AutoSendingCrashListener : CrashManagerListener
    {
        public override string UserID
        {
            get
            {
                var principal = Mvx.Resolve<IPrincipal>();
                return principal.CurrentUserIdentity?.UserId.ToString();
            }
        }

        public override string Description
        {
            get
            {
                var serviceSettings = Mvx.Resolve<IRestServiceSettings>();
                var endpoint = serviceSettings.Endpoint;

                var principal = Mvx.Resolve<IPrincipal>();
                var userName = principal.CurrentUserIdentity?.Name;
                return $"Endpoint: {endpoint};{Environment.NewLine}User name: {userName}";
            }
        }
    
        public override bool ShouldAutoUploadCrashes() => true;
    }
}