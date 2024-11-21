using Android.App;
using Android.Views;
using Java.Interop;
using MvvmCross.Commands;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.ViewModels;
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
                EnumeratorUIResources.MenuItem_Title_Dashboard,
                new MvxAsyncCommand(async () =>
                {
                    var screen = this.ViewModel.SourceScreen;
                    switch (screen)
                    {
                        case SourceScreen.AssignmentMap:
                            await this.ViewModel.NavigateToAssigmentMapCommand.ExecuteAsync();
                            break;
                        case SourceScreen.MapDashboard:
                            await this.ViewModel.NavigateToMapDashboardCommand.ExecuteAsync();
                            break;
                        default:
                            await this.ViewModel.NavigateToDashboardCommand.ExecuteAsync();
                            break;
                    }
                    
                    this.ReleaseActivity();
                })
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
            {
                Resource.Id.menu_maps,
                EnumeratorUIResources.MenuItem_Title_Maps,
                this.ViewModel.NavigateToMapsCommand
            },
            {
                Resource.Id.interview_language,
                EnumeratorUIResources.MenuItem_Title_Language
            },
            {
                Resource.Id.interview_language_original,
                this.ViewModel.DefaultLanguageName ?? EnumeratorUIResources.MenuItem_Title_Language_Original
            },
        };

        [Export("NavigateToApi")]
        public void NavigateToApi(string navigateTo)
        {
            base.Navigate(navigateTo);
        }
    }
}
