using Cheesebaron.MvxPlugins.Settings.Interfaces;
using IHS.MvvmCross.Plugins.Keychain;
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

        private Identity currentIdentity;
        public IIdentity CurrentIdentity { get { return currentIdentity; } }

        public Principal(IKeychain securityService, ISettings settingsService)
        {
            this.securityService = securityService;
            this.settingsService = settingsService;

            this.InitializeIdentity();
        }

        private void InitializeIdentity()
        {
            var userName = this.settingsService.GetValue(UserNameParameterName, string.Empty);
            
            this.currentIdentity = new Identity()
            {
                IsAuthenticated = !string.IsNullOrEmpty(userName),
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

            this.currentIdentity.IsAuthenticated = true;
            this.currentIdentity.Name = usernName;
            this.currentIdentity.Password = password;
        }

        public void SignOut()
        {
            this.settingsService.DeleteValue(UserNameParameterName);
            this.securityService.DeleteAccount(ServiceParameterName, this.currentIdentity.Name);

            this.currentIdentity.IsAuthenticated = false;
            this.currentIdentity.Name = string.Empty;
            this.currentIdentity.Password = string.Empty;
        }
    }
}