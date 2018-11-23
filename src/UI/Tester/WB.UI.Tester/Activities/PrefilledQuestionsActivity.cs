using Android.App;
using Android.Views;
using WB.Core.BoundedContexts.Tester.Properties;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.UI.Shared.Enumerator.Activities;

namespace WB.UI.Tester.Activities
{
    [Activity(Label = "", Theme = "@style/BlueAppTheme",
        HardwareAccelerated = true,
        WindowSoftInputMode = SoftInput.StateAlwaysHidden | SoftInput.AdjustPan,
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize,
        Exported = false)]
    public class PrefilledQuestionsActivity : BasePrefilledQuestionsActivity<PrefilledQuestionsViewModel>
    {
        protected override int LanguagesMenuGroupId => Resource.Id.prefilled_questions_languages;
        protected override int OriginalLanguageMenuItemId => Resource.Id.prefilled_questions_language_original;
        protected override int LanguagesMenuItemId => Resource.Id.prefilled_questions_language;
        protected override int MenuId => Resource.Menu.prefilled_questions;

        protected override MenuDescription MenuDescriptor => new MenuDescription
        {
            {
                Resource.Id.interview_reload,
                TesterUIResources.MenuItem_Title_Reload,
                this.ViewModel.ReloadQuestionnaireCommand
            },
            {
                Resource.Id.prefilled_questions_dashboard,
                TesterUIResources.MenuItem_Title_Dashboard,
                this.ViewModel.NavigateToDashboardCommand
            },
            {
                Resource.Id.prefilled_questions_settings,
                TesterUIResources.MenuItem_Title_Settings,
                this.ViewModel.NavigateToSettingsCommand
            },
            {
                Resource.Id.prefilled_questions_signout,
                TesterUIResources.MenuItem_Title_SignOut,
                this.ViewModel.SignOutCommand
            },
            {
                Resource.Id.prefilled_questions_language,
                TesterUIResources.MenuItem_Title_Language
            },
            {
                Resource.Id.prefilled_questions_language_original,
                TesterUIResources.MenuItem_Title_Language_Original
            },
        };
    }
}
