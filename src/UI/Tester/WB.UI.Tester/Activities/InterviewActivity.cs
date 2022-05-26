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
                Resource.Id.interview_anonymous_questionnaire,
                TesterUIResources.MenuItem_Title_AnonymousQuestionnaires,
                this.ViewModel.NavigateToAnonymousQuestionnairesCommand
            },
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
                Resource.Id.interview_login,
                TesterUIResources.MenuItem_Title_Login,
                this.ViewModel.NavigateToLoginCommand
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
                this.ViewModel.DefaultLanguageName ?? TesterUIResources.MenuItem_Title_Language_Original
            },
        };

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            var onCreateOptionsMenu = base.OnCreateOptionsMenu(menu);

            var isAuthenticated = this.ViewModel.IsAuthenticated;
            menu.VisibleMenuItem(Resource.Id.interview_login, !isAuthenticated);
            menu.VisibleMenuItem(Resource.Id.interview_signout, isAuthenticated);
            menu.VisibleMenuItem(Resource.Id.interview_anonymous_questionnaire, !isAuthenticated);
            menu.VisibleMenuItem(Resource.Id.interview_dashboard, isAuthenticated);
            
            return onCreateOptionsMenu;
        }

        protected override void OnResume()
        {
            base.OnResume();

            if (!this.ViewModel.IsSuccessfullyLoaded) return;

            var shouldShowVariables = this.ViewModel.EnumeratorSettings.ShowVariables;
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
