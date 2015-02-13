using System.Collections.Generic;
using System.Linq;

using Android.Content.Res;

using Chance.MvvmCross.Plugins.UserInteraction;

using Cirrious.CrossCore;
using Cirrious.CrossCore.Droid.Platform;
using Cirrious.MvvmCross.ViewModels;
using Main.Core.Entities.SubEntities;
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

        private IPasswordHasher passwordHasher
        {
            get { return ServiceLocator.Current.GetInstance<IPasswordHasher>(); }
        }

        public string Login { get; private set; }
        public string Password { get; private set; }

        public string KnownUsers {
            get { return string.Join(", ", Users.Select(x => x.Name)); }
        }

        public List<UserLight> Users { get; private set; } 

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
            Users = CapiApplication.Membership.GetKnownUsers().Result;
            RaisePropertyChanged(() => this.KnownUsers);
#if DEBUG
            this.Login = "inter";
            this.Password = "Qwerty1234";
#endif
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

            var activityContext = Mvx.Resolve<IMvxAndroidCurrentTopActivity>().Activity;

            Mvx.Resolve<IUserInteraction>().Confirm(
                    activityContext.GetString(Resource.String.AryYouNewForThisTablet),
                    okButton: activityContext.GetString(Resource.String.Yes),
                    cancelButton: activityContext.GetString(Resource.String.No),
                    okClicked: () => this.NavigationService.NavigateTo(
                                CapiPages.Synchronization,
                                new Dictionary<string, string>
                                {
                                    { "Login", this.Login },
                                    { "PasswordHash", this.passwordHasher.Hash(this.Password) }
                                }));
        }
    }
}