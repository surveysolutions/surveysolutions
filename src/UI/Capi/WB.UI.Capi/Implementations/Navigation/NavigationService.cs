using System;
using System.Collections.Generic;

using Android.App;
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

        public void NavigateTo(CapiPages navigateToPage, Dictionary<string, string> pageParameters, bool clearHistory = false)
        {
            var activityContext = Mvx.Resolve<IMvxAndroidCurrentTopActivity>().Activity;

            var targetActivity = new Intent(activityContext, PageToTypeMapping[navigateToPage]);

            if (clearHistory)
            {
                targetActivity.AddFlags(ActivityFlags.ClearTask);
                targetActivity.AddFlags(ActivityFlags.ClearTop);
                targetActivity.AddFlags(ActivityFlags.NewTask);
            }

            foreach (var parameterName in pageParameters.Keys)
            {
                targetActivity.PutExtra(parameterName, pageParameters[parameterName]);
            }

            activityContext.StartActivity(targetActivity);

            if (clearHistory)
            {
                activityContext.Finish();
            }
        }
    }
}