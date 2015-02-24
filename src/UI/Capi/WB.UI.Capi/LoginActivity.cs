using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Content.PM;
using Cirrious.MvvmCross.Droid.Views;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Capi.Services;
using WB.Core.BoundedContexts.Capi.ValueObjects;
using WB.UI.Capi.Extensions;
using WB.UI.Capi.Views;

namespace WB.UI.Capi
{
    [Activity(ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class LoginActivity : MvxActivity
    {
        private IInterviewerSettings interviewerSettings
        {
            get { return ServiceLocator.Current.GetInstance<IInterviewerSettings>(); }
        }

        private INavigationService NavigationService
        {
            get { return ServiceLocator.Current.GetInstance<INavigationService>(); }
        }

        private bool isResumeFirstRun = true;
        protected override void OnStart()
        {
            base.OnStart();
            this.CreateActionBar();
        }

        protected override void OnCreate(Bundle bundle)
        {
            if (CapiApplication.Membership.IsLoggedIn)
            {
                NavigationService.NavigateTo(CapiPages.Dashboard, new Dictionary<string, string>());
                this.Finish();
            }

            if (interviewerSettings.GetClientRegistrationId() == null)
            {
                NavigationService.NavigateTo(CapiPages.FinishInstallation, new Dictionary<string, string>(), true);
                this.Finish();
            }

            this.DataContext = new LoginActivityViewModel();
            base.OnCreate(bundle);
            this.SetContentView(Resource.Layout.Login);
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (isResumeFirstRun)
            {
                isResumeFirstRun = false;
                return;
            }
            var viewModel = this.DataContext as LoginActivityViewModel;
            if(viewModel==null)
                return;
            viewModel.UpdateViewModel();
        }
    }
}