using Android.Content;
using Android.Content.PM;
using Android.Views;
using Android.Widget;
using AndroidX.Biometric;
using AndroidX.Core.Content;
using Java.Interop;
using MvvmCross;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.UI.Shared.Enumerator.Activities;
using WB.UI.Shared.Enumerator.Services.Notifications;
using Toolbar=AndroidX.AppCompat.Widget.Toolbar;

namespace WB.UI.Interviewer.Activities
{
    [Activity(WindowSoftInputMode = SoftInput.StateHidden, 
        Theme = "@style/GrayAppTheme",
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize,
        Exported = false)]
    public class LoginActivity : BaseActivity<LoginViewModel>
    {
        private BiometricPrompt biometricPrompt;
        private BiometricPrompt.PromptInfo biometricPromptInfo;

        protected override int ViewResourceId
        {
            get { return Resource.Layout.Login; }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            var toolbar = this.FindViewById<Toolbar>(Resource.Id.toolbar);
            toolbar.Title = "";
            this.SetSupportActionBar(toolbar);
            this.InitializeBiometricLogin();
        }
        
        protected override bool BackButtonCustomAction => true;
        protected override void BackButtonPressed()
        {
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(Resource.Menu.login, menu);

            menu.LocalizeMenuItem(Resource.Id.menu_settings, EnumeratorUIResources.MenuItem_Title_Settings);

            menu.LocalizeMenuItem(Resource.Id.menu_diagnostics, EnumeratorUIResources.MenuItem_Title_Diagnostics);
            return base.OnCreateOptionsMenu(menu);
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menu_settings:
                    Intent intent = new Intent(this, typeof(PrefsActivity));
                    this.StartActivity(intent);
                    break;
                case Resource.Id.menu_diagnostics:
                    this.ViewModel.NavigateToDiagnosticsPageCommand.Execute();
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }

        protected override void OnResume()
        {
            base.OnResume();
         
            var notificationsPublisher = Mvx.IoCProvider.Resolve<INotificationPublisher>();
            notificationsPublisher.CancelAllNotifications(this);
        }
        
        [Export("FastLogin")]
        public void FastLogin(string password)
        {
            this.ViewModel.Password = password;
            this.ViewModel.SignInCommand.ExecuteAsync();
        }

        private void InitializeBiometricLogin()
        {
            var biometricLoginButton = this.FindViewById<ImageButton>(Resource.Id.login_biometric_button);
            if (biometricLoginButton == null)
                return;

            biometricLoginButton.Visibility = ViewStates.Gone;
            biometricLoginButton.ContentDescription = EnumeratorUIResources.LoginText;

            if (BiometricManager.From(this).CanAuthenticate() != BiometricManager.BiometricSuccess)
                return;

            this.biometricPrompt = new BiometricPrompt(this,
                ContextCompat.GetMainExecutor(this),
                new BiometricAuthenticationCallback(this.ViewModel));
            this.biometricPromptInfo = new BiometricPrompt.PromptInfo.Builder()
                .SetTitle(EnumeratorUIResources.LoginText)
                .SetSubtitle(EnumeratorUIResources.PasswordHint)
                .SetNegativeButtonText(EnumeratorUIResources.Cancel)
                .Build();

            biometricLoginButton.Visibility = ViewStates.Visible;
            biometricLoginButton.Click += (sender, args) => this.biometricPrompt.Authenticate(this.biometricPromptInfo);
        }

        private sealed class BiometricAuthenticationCallback : BiometricPrompt.AuthenticationCallback
        {
            private readonly LoginViewModel viewModel;

            public BiometricAuthenticationCallback(LoginViewModel viewModel)
            {
                this.viewModel = viewModel;
            }

            public override void OnAuthenticationSucceeded(BiometricPrompt.AuthenticationResult result)
            {
                base.OnAuthenticationSucceeded(result);
                this.viewModel.SignInWithHashCommand.ExecuteAsync();
            }
        }
    }
}
