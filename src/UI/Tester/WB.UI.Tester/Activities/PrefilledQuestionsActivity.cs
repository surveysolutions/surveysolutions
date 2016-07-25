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
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class PrefilledQuestionsActivity : BasePrefilledQuestionsActivity<PrefilledQuestionsViewModel>
    {
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(Resource.Menu.interview, menu);

            menu.LocalizeMenuItem(Resource.Id.interview_dashboard, TesterUIResources.MenuItem_Title_Dashboard);
            menu.LocalizeMenuItem(Resource.Id.interview_settings, TesterUIResources.MenuItem_Title_Settings);
            menu.LocalizeMenuItem(Resource.Id.interview_signout, TesterUIResources.MenuItem_Title_SignOut);
            menu.LocalizeMenuItem(Resource.Id.interview_language, TesterUIResources.MenuItem_Title_Language);
            menu.LocalizeMenuItem(Resource.Id.interview_language_original, TesterUIResources.MenuItem_Title_Language_Original);

            this.PopulateLanguagesMenu(menu,
                Resource.Id.interview_language, Resource.Id.interview_language_original, Resource.Id.interview_languages);

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.interview_dashboard:
                    this.ViewModel.NavigateToDashboardCommand.Execute();
                    return true;

                case Resource.Id.interview_settings:
                    this.ViewModel.NavigateToSettingsCommand.Execute();
                    return true;

                case Resource.Id.interview_signout:
                    this.ViewModel.SignOutCommand.Execute();
                    return true;

                default:
                    if (item.GroupId == Resource.Id.interview_languages && !item.IsChecked)
                    {
                        var language =
                            item.ItemId == Resource.Id.interview_language_original
                                ? null
                                : item.TitleFormatted.ToString();

                        this.ViewModel.SwitchTranslationCommand.Execute(language);
                        this.ViewModel.ReloadPrefilledQuestionsCommand.Execute();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
            }
        }
    }
}