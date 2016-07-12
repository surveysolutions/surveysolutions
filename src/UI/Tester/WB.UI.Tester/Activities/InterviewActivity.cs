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
    public class InterviewActivity : EnumeratorInterviewActivity<InterviewViewModel>
    {
        public override void OnBackPressed()
        {
            this.ViewModel.NavigateToPreviousViewModel(() =>
            {
                this.ViewModel.NavigateBack();
                this.Finish();
            });
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(Resource.Menu.interview, menu);

            menu.LocalizeMenuItem(Resource.Id.interview_dashboard, TesterUIResources.MenuItem_Title_Dashboard);
            menu.LocalizeMenuItem(Resource.Id.interview_settings, TesterUIResources.MenuItem_Title_Settings);
            menu.LocalizeMenuItem(Resource.Id.interview_signout, TesterUIResources.MenuItem_Title_SignOut);
            menu.LocalizeMenuItem(Resource.Id.interview_language, TesterUIResources.MenuItem_Title_Language);
            menu.LocalizeMenuItem(Resource.Id.interview_language_original, TesterUIResources.MenuItem_Title_Language_Original);

            ISubMenu languagesMenu = menu.FindItem(Resource.Id.interview_language).SubMenu;

            languagesMenu.Add(
                groupId: Resource.Id.interview_languages,
                itemId: Menu.None,
                order: Menu.None,
                title: "English");

            languagesMenu.Add(
                groupId: Resource.Id.interview_languages,
                itemId: Menu.None,
                order: Menu.None,
                title: "Русский");

            languagesMenu.SetGroupCheckable(Resource.Id.interview_languages, checkable: true, exclusive: true);

            menu.FindItem(Resource.Id.interview_language_original).SetChecked(true);

            return base.OnCreateOptionsMenu(menu);
        }

        protected override void OnMenuItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.interview_dashboard:
                    this.ViewModel.NavigateToDashboardCommand.Execute();
                    break;

                case Resource.Id.interview_settings:
                    this.ViewModel.NavigateToSettingsCommand.Execute();
                    break;

                case Resource.Id.interview_signout:
                    this.ViewModel.SignOutCommand.Execute();
                    break;

                default:
                    if (item.GroupId == Resource.Id.interview_languages)
                    {
                        item.SetChecked(true);
                    }
                    break;
            }
        }
    }
}