using Android.App;
using Android.Views;
using Java.Interop;
using WB.Core.BoundedContexts.Tester.Properties;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.Enumerator;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.UI.Shared.Enumerator.Activities;

namespace WB.UI.Tester.Activities
{
    [Activity(Label = "", Theme = "@style/BlueAppTheme", HardwareAccelerated = true,
        WindowSoftInputMode = SoftInput.StateAlwaysHidden | SoftInput.AdjustPan,
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class InterviewActivity : BaseInterviewActivity<InterviewViewModel>
    {
        protected override int LanguagesMenuGroupId => Resource.Id.interview_languages;
        protected override int OriginalLanguageMenuItemId => Resource.Id.interview_language_original;
        protected override int LanguagesMenuItemId => Resource.Id.interview_language;
        protected override int MenuId => Resource.Menu.interview;

        protected override MenuDescription MenuDescriptor => new MenuDescription
        {
            {
                Resource.Id.interview_dashboard,
                TesterUIResources.MenuItem_Title_Dashboard,
                this.ViewModel.NavigateToDashboardCommand
            },
            {
                Resource.Id.interview_settings,
                TesterUIResources.MenuItem_Title_Settings,
                this.ViewModel.NavigateToSettingsCommand
            },
            {
                Resource.Id.interview_reload,
                TesterUIResources.MenuItem_Title_Reload,
                this.ViewModel.ReloadQuestionnaireCommand
            },
            {
                Resource.Id.interview_signout,
                TesterUIResources.MenuItem_Title_SignOut,
                this.ViewModel.SignOutCommand
            },
            {
                Resource.Id.interview_language,
                TesterUIResources.MenuItem_Title_Language
            },
            {
                Resource.Id.interview_language_original,
                TesterUIResources.MenuItem_Title_Language_Original
            },
        };

        protected override void OnResume()
        {
            base.OnResume();

            if (!this.ViewModel.IsSuccessfullyLoaded) return;

            var shouldShowVariables = ServiceLocator.Current.GetInstance<IEnumeratorSettings>()?.ShowVariables ?? false;
            if (this.ViewModel.IsVariablesShowed != shouldShowVariables)
                this.ViewModel?.ReloadCommand?.Execute();  
        }

        [Export("NavigateToApi")]
        public void NavigateToApi(string navigateTo)
        {
            base.Navigate(navigateTo);
        }
    }
}
