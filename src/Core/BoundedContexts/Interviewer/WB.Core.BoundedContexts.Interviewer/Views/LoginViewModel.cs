using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;
using System.Threading.Tasks;
using MvvmCross.Commands;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.Views;
using Xamarin.Essentials;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class LoginViewModel : EnumeratorLoginViewModel
    {
        private readonly IInterviewerPrincipal interviewerPrincipal;

        public LoginViewModel(
            IViewModelNavigationService viewModelNavigationService,
            IInterviewerPrincipal principal,
            IPasswordHasher passwordHasher,
            IPlainStorage<CompanyLogo> logoStorage,
            IOnlineSynchronizationService synchronizationService,
            ILogger logger,
            IAuditLogService auditLogService)
            : base(viewModelNavigationService, principal, passwordHasher, logoStorage, synchronizationService, logger, auditLogService)
        {
            this.interviewerPrincipal = principal;
        }

        public override bool HasUser() => this.interviewerPrincipal.DoesIdentityExist();

        public override string? GetUserName()
            => this.interviewerPrincipal.GetExistingIdentityNameOrNull();

        public override void UpdateLocalUser(string userName, string token, string passwordHash)
        {
            var localInterviewer = this.interviewerPrincipal.GetInterviewerByName(userName);
            localInterviewer.Token = token;
            localInterviewer.PasswordHash = passwordHash;

            this.interviewerPrincipal.SaveInterviewer(localInterviewer);
        }

        public override async Task<bool> CanDoBiometricLogin()
        {
            try
            {
                if (await SecureStorage.GetAsync("userName") == null ||
                    await SecureStorage.GetAsync("userPassword") == null)
                {
                    return false;
                }

                var availability = await CrossFingerprint.Current.GetAvailabilityAsync();
                if (availability != FingerprintAvailability.Available)
                {
                    return false;
                }

                return await CrossFingerprint.Current.IsAvailableAsync();
            }
            catch
            {
                return false;
            }
        }

        protected override async Task OnSuccessfulLogin(string userName, string password)
        {
            await SecureStorage.SetAsync("userName", userName);
            await SecureStorage.SetAsync("userPassword", password);
        }

        public IMvxAsyncCommand BiometricSignInCommand => new MvxAsyncCommand(this.DoBiometricLogin);

        public override async Task<bool> DoBiometricLogin()
        {
            if (await CanDoBiometricLogin())
            {
                var request = new AuthenticationRequestConfiguration(UIResources.Interviewer_ApplicationName, UIResources.BiometricAuthentication)
                {
                    // Setting this true, will force only authorization with Biometrics or PIN,
                    // there will be no option to cancel
                    AllowAlternativeAuthentication = false,
                };

                var result = await CrossFingerprint.Current.AuthenticateAsync(request);

                if (result.Authenticated)
                {
                    this.Password = await SecureStorage.GetAsync("userPassword");
                    await this.SignIn();
                    return true;
                }
            }

            return false;
        }
    }
}
