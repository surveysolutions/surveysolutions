using Android.App;
using Android.Views;
using Java.Interop;
using WB.Core.BoundedContexts.Supervisor.ViewModel;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.UI.Shared.Enumerator.Activities;

namespace WB.UI.Supervisor.Activities.Interview
{
    [Activity(Label = "", Theme = "@style/BlueAppTheme",
        HardwareAccelerated = true,
        WindowSoftInputMode = SoftInput.StateAlwaysHidden | SoftInput.AdjustPan,
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize,
        Exported = false)]
    public class PrefilledQuestionsActivity : BasePrefilledQuestionsActivity<PrefilledQuestionsViewModel>
    {
        protected override int LanguagesMenuGroupId => Resource.Id.interview_languages;
        protected override int OriginalLanguageMenuItemId => Resource.Id.interview_language_original;
        protected override int LanguagesMenuItemId => Resource.Id.interview_language;
        protected override int MenuId => Resource.Menu.interview;

        protected override MenuDescription MenuDescriptor => new MenuDescription
        {
            {
                Resource.Id.menu_dashboard,
                EnumeratorUIResources.MenuItem_Title_Dashboard,
                this.ViewModel.NavigateToDashboardCommand
            },
            {
                Resource.Id.menu_signout,
                EnumeratorUIResources.MenuItem_Title_SignOut,
                this.ViewModel.SignOutCommand
            },
            {
                Resource.Id.menu_diagnostics,
                EnumeratorUIResources.MenuItem_Title_Diagnostics,
                this.ViewModel.NavigateToDiagnosticsPageCommand
            },
            //{
            //    Resource.Id.menu_maps,
            //    InterviewerUIResources.MenuItem_Title_Maps,
            //    this.ViewModel.NavigateToMapsCommand
            //},
            {
                Resource.Id.interview_language,
                EnumeratorUIResources.MenuItem_Title_Language
            },
            {
                Resource.Id.interview_language_original,
                EnumeratorUIResources.MenuItem_Title_Language_Original
            },
        };
        
        [Export("StartInterviewApi")]
        public void StartInterviewApi()
        {
            this.ViewModel.StartInterviewAsync().Wait();
        }
    }
}
