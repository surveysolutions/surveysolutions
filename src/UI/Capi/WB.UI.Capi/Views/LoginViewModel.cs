using System.Collections.Generic;

using Chance.MvvmCross.Plugins.UserInteraction;

using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Capi.Services;
using WB.Core.BoundedContexts.Capi.ValueObjects;
using WB.Core.GenericSubdomains.Utils;
using WB.UI.Capi.Settings;

namespace WB.UI.Capi.Views
{
    public class LoginActivityViewModel : MvxViewModel
    {
        private INavigationService NavigationService
        {
            get { return ServiceLocator.Current.GetInstance<INavigationService>(); }
        }

        private IPasswordHasher passwordHasher
        {
            get { return ServiceLocator.Current.GetInstance<IPasswordHasher>(); }
        }

        private IInterviewerSettings interviewerSettings
        {
            get { return ServiceLocator.Current.GetInstance<IInterviewerSettings>(); }
        }

        public string Login { get; private set; }
        public string Password { get; private set; }

        private bool isLoginValid = true;
        public bool IsLoginValid
        {
            get { return this.isLoginValid; }
            set { this.isLoginValid = value; RaisePropertyChanged(() => this.IsLoginValid); }
        }

        private bool isPasswordValid = true;
        public bool IsPasswordValid
        {
            get { return this.isPasswordValid; }
            set { this.isPasswordValid = value; RaisePropertyChanged(() => this.IsPasswordValid); }
        }

        public LoginActivityViewModel()
        {
#if DEBUG
            this.Login = "inter";
            this.Password = "P@$$w0rd";
#endif

            if (CapiApplication.Membership.IsLoggedIn)
            {
                NavigationService.NavigateTo(CapiPages.Dashboard, new Dictionary<string, string>());
            }

            if (interviewerSettings.GetClientRegistrationId() == null)
            {
                NavigationService.NavigateTo(CapiPages.FinishInstallation, new Dictionary<string, string>(), true);
            }
        }

        public IMvxCommand LoginCommand
        {
            get { return new MvxCommand(this.StartLogin); }
        }

        private void StartLogin()
        {
            var result = CapiApplication.Membership.LogOnAsync(Login, Password).Result;
            if (result)
            {
                NavigationService.NavigateTo(CapiPages.Dashboard, new Dictionary<string, string>(), true);
                return;
            }

            IsLoginValid = false;
            IsPasswordValid = false;

            Mvx.Resolve<IUserInteraction>().Confirm("Are you sure new user for this tablet?",
                    async () => this.NavigationService.NavigateTo(CapiPages.Synchronization,
                            new Dictionary<string, string>
                            {
                                { "Login", this.Login },
                                { "PasswordHash", this.passwordHasher.Hash(this.Password) }
                            }));
        }
    }
}