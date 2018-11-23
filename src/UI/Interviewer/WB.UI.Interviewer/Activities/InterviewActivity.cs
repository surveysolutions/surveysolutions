using Android.App;
using Android.Views;
using Java.Interop;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.UI.Interviewer.ViewModel;
using WB.UI.Shared.Enumerator.Activities;

namespace WB.UI.Interviewer.Activities
{
    [Activity(Label = "",
        Theme = "@style/BlueAppTheme",
        HardwareAccelerated = true,
        WindowSoftInputMode = SoftInput.StateAlwaysHidden | SoftInput.AdjustPan,
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize,
        Exported = false)]
    public class InterviewActivity : BaseInterviewActivity<InterviewViewModel>
    {
        protected override int LanguagesMenuGroupId => Resource.Id.interview_languages;
        protected override int OriginalLanguageMenuItemId => Resource.Id.interview_language_original;
        protected override int LanguagesMenuItemId => Resource.Id.interview_language;
        protected override int MenuId => Resource.Menu.interview;

        protected override MenuDescription MenuDescriptor => new MenuDescription
        {
            {
                Resource.Id.menu_dashboard,
                InterviewerUIResources.MenuItem_Title_Dashboard,
                this.ViewModel.NavigateToDashboardCommand
            },
            {
                Resource.Id.menu_signout,
                InterviewerUIResources.MenuItem_Title_SignOut,
                this.ViewModel.SignOutCommand
            },
            {
                Resource.Id.menu_diagnostics,
                InterviewerUIResources.MenuItem_Title_Diagnostics,
                this.ViewModel.NavigateToDiagnosticsPageCommand
            },
            {
                Resource.Id.menu_maps,
                InterviewerUIResources.MenuItem_Title_Maps,
                this.ViewModel.NavigateToMapsCommand
            },
            {
                Resource.Id.interview_language,
                InterviewerUIResources.MenuItem_Title_Language
            },
            {
                Resource.Id.interview_language_original,
                InterviewerUIResources.MenuItem_Title_Language_Original
            },
        };

        [Export("NavigateToApi")]
        public void NavigateToApi(string navigateTo)
        {
            base.Navigate(navigateTo);
        }
    }
}
