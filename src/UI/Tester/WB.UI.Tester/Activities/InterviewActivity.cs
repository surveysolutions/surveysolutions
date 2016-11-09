using Android.App;
using Android.Views;
using WB.Core.BoundedContexts.Tester.Properties;
using WB.Core.BoundedContexts.Tester.ViewModels;
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
    }
}