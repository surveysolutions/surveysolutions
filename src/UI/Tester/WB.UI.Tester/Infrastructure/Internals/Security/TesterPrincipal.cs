using System;
using IHS.MvvmCross.Plugins.Keychain;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using ISettings = Cheesebaron.MvxPlugins.Settings.Interfaces.ISettings;

namespace WB.UI.Tester.Infrastructure.Internals.Security
{
    internal class TesterPrincipal : IPrincipal
    {
        public const string ServiceParameterName = "authentication";
        private const string UserNameParameterName = "authenticatedUser";

        private readonly IKeychain securityService;
        private readonly ISettings settingsService;

        private UserIdentity currentUserIdentity;
        public bool IsAuthenticated { get; private set; }
        public IUserIdentity CurrentUserIdentity { get { return this.currentUserIdentity; } }

        public TesterPrincipal(IKeychain securityService, ISettings settingsService)
        {
            this.securityService = securityService;
            this.settingsService = settingsService;

            this.InitializeIdentity();
        }

        private void InitializeIdentity()
        {
            var userName = this.settingsService.GetValue(UserNameParameterName, string.Empty);

            this.IsAuthenticated = !string.IsNullOrEmpty(userName);
            this.currentUserIdentity = new UserIdentity()
            {
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

            this.IsAuthenticated = true;
            this.currentUserIdentity.Name = usernName;
            this.currentUserIdentity.Password = password;
        }

        public void SignOut()
        {
            this.settingsService.DeleteValue(UserNameParameterName);
            this.securityService.DeleteAccount(ServiceParameterName, this.currentUserIdentity.Name);

            this.IsAuthenticated = false;
            this.currentUserIdentity.Name = string.Empty;
            this.currentUserIdentity.Password = string.Empty;
        }
    }
}