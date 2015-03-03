using System.Collections.Generic;
using System.Linq;

using Android.App;
using Cirrious.MvvmCross.ViewModels;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Capi.Services;
using WB.Core.BoundedContexts.Capi.ValueObjects;
using WB.Core.GenericSubdomains.Utils;

namespace WB.UI.Capi.Views
{
    public class LoginActivityViewModel : MvxViewModel
    {
        private INavigationService NavigationService
        {
            get { return ServiceLocator.Current.GetInstance<INavigationService>(); }
        }

        private IDataCollectionAuthentication DataCollectionAuthentication
        {
            get { return ServiceLocator.Current.GetInstance<IDataCollectionAuthentication>(); }
        }

        private IPasswordHasher passwordHasher
        {
            get { return ServiceLocator.Current.GetInstance<IPasswordHasher>(); }
        }

        public string Login { get; private set; }
        public string Password { get; private set; }

        public string KnownUsers {
            get { return string.Format("{0}: {1}", Application.Context.GetString(Resource.String.ActiveUsers), string.Join(", ", this.Logins)); }
        }

        public List<string> Logins { get; private set; } 

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

        private bool areCredentialsWrong = false;
        public bool AreCredentialsWrong
        {
            get { return this.areCredentialsWrong; }
            set { this.areCredentialsWrong = value; RaisePropertyChanged(() => this.AreCredentialsWrong); }
        }

        private bool shouldActivationButtonBeVisible = false;
        public bool ShouldActivationButtonBeVisible
        {
            get { return this.shouldActivationButtonBeVisible; }
            set { this.shouldActivationButtonBeVisible = value; RaisePropertyChanged(() => this.ShouldActivationButtonBeVisible); }
        }

        private int countOfUnsuccessfulLogins = 0;

        public LoginActivityViewModel()
        {
#if DEBUG
            this.Login = "inter";
            this.Password = "Qwerty1234";
#endif
        }

        public void Init()
        {
            this.Logins = DataCollectionAuthentication.GetKnownUsers().Result;
            RaisePropertyChanged(() => this.KnownUsers);
            if (this.Logins.Count == 1)
            {
                this.Login = this.Logins.First();
            }
            this.countOfUnsuccessfulLogins = 0;
            ShouldActivationButtonBeVisible = false;
        }

        public IMvxCommand LoginCommand
        {
            get { return new MvxCommand(this.StartLogin); }
        }

        public IMvxCommand ActivationCommand
        {
            get { return new MvxCommand(this.StartActivation); }
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
            AreCredentialsWrong = true;
            this.countOfUnsuccessfulLogins++;
            if (this.countOfUnsuccessfulLogins > 3)
            {
                ShouldActivationButtonBeVisible = true;
            }
        }

        private void StartActivation()
        {
            this.NavigationService.NavigateTo(CapiPages.Synchronization,
                new Dictionary<string, string>
                {
                    { "Login", this.Login },
                    { "PasswordHash", this.passwordHasher.Hash(this.Password) }
                });
        }
    }
}