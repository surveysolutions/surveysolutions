using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Android.Content;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Droid.Platform;
using WB.Core.BoundedContexts.Capi.Services;
using WB.Core.BoundedContexts.Capi.ValueObjects;

namespace WB.UI.Capi.Implementations.Navigation
{
    internal class NavigationService : INavigationService
    {
        private static readonly Dictionary<CapiPages, Type> PageToTypeMapping = new Dictionary<CapiPages, Type>
        {
           { CapiPages.Splash, typeof(SplashScreen)},
           { CapiPages.Login , typeof(LoginActivity)},
           { CapiPages.FinishInstallation, typeof(FinishInstallationActivity)},
           { CapiPages.Dashboard , typeof(DashboardActivity)},
           { CapiPages.Settings, typeof(SettingsActivity)},
           { CapiPages.Synchronization, typeof(SynchronizationActivity)}
        };

        public void NavigateTo(CapiPages navigateToPage, NameValueCollection pageParameters)
        {
            var activityContext = Mvx.Resolve<IMvxAndroidCurrentTopActivity>().Activity;

            var targetActivity = new Intent(activityContext, PageToTypeMapping[navigateToPage]);

            foreach (var parameterName in pageParameters.AllKeys)
            {
                targetActivity.PutExtra(parameterName, pageParameters[parameterName]);
            }
            activityContext.StartActivity(targetActivity);
        }
    }
}