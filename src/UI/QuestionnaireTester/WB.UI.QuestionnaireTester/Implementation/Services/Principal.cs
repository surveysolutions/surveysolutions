using System;
using Cheesebaron.MvxPlugins.Settings.Interfaces;
using IHS.MvvmCross.Plugins.Keychain;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.UI.QuestionnaireTester.Implementation.Views;

namespace WB.UI.QuestionnaireTester.Implementation.Services
{
    internal class Principal : IPrincipal
    {
        public const string ServiceParameterName = "authentication";
        private const string UserNameParameterName = "authenticatedUser";

        private readonly IKeychain securityService;
        private readonly ISettings settingsService;

        private UserIdentity currentUserIdentity;
        public IUserIdentity CurrentUserIdentity { get { return currentUserIdentity; } }

        public Principal(IKeychain securityService, ISettings settingsService)
        {
            this.securityService = securityService;
            this.settingsService = settingsService;

            this.InitializeIdentity();
        }

        private void InitializeIdentity()
        {
            var userName = this.settingsService.GetValue(UserNameParameterName, string.Empty);
            
            this.currentUserIdentity = new UserIdentity()
            {
                IsAuthenticated = !string.IsNullOrEmpty(userName),
                UserId = Guid.NewGuid(),
                Name = userName,
                Password = this.securityService.GetPassword(ServiceParameterName, userName)
            };
        }

        public void SignIn(string usernName, string password, bool staySignedIn)
        {
            if (staySignedIn)
            {
                this.settingsService.AddOrUpdateValue(UserNameParameterName, usernName);
                this.securityService.SetPassword(password, ServiceParameterName, usernName);
            }

            this.currentUserIdentity.IsAuthenticated = true;
            this.currentUserIdentity.Name = usernName;
            this.currentUserIdentity.Password = password;
        }

        public void SignOut()
        {
            this.settingsService.DeleteValue(UserNameParameterName);
            this.securityService.DeleteAccount(ServiceParameterName, this.currentUserIdentity.Name);

            this.currentUserIdentity.IsAuthenticated = false;
            this.currentUserIdentity.Name = string.Empty;
            this.currentUserIdentity.Password = string.Empty;
        }
    }
}