using System;
using System.Linq;
using Android.App;
using Android.Views;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.UI.Interviewer.ViewModel;
using WB.UI.Shared.Enumerator.Activities;

namespace WB.UI.Interviewer.Activities
{
    [Activity(Label = "", Theme = "@style/BlueAppTheme", HardwareAccelerated = true,
        WindowSoftInputMode = SoftInput.StateAlwaysHidden | SoftInput.AdjustPan,
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class InterviewActivity : EnumeratorInterviewActivity<InterviewerInterviewViewModel>
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

            menu.LocalizeMenuItem(Resource.Id.menu_dashboard, InterviewerUIResources.MenuItem_Title_Dashboard);
            menu.LocalizeMenuItem(Resource.Id.menu_signout, InterviewerUIResources.MenuItem_Title_SignOut);
            menu.LocalizeMenuItem(Resource.Id.menu_diagnostics, InterviewerUIResources.MenuItem_Title_Diagnostics);

            menu.LocalizeMenuItem(Resource.Id.interview_language, InterviewerUIResources.MenuItem_Title_Language);
            menu.LocalizeMenuItem(Resource.Id.interview_language_original, InterviewerUIResources.MenuItem_Title_Language_Original);

            this.PopulateLanguagesMenu(menu,
                Resource.Id.interview_language, Resource.Id.interview_language_original, Resource.Id.interview_languages);

            return base.OnCreateOptionsMenu(menu);
        }

        protected override void OnMenuItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menu_dashboard:
                    this.ViewModel.NavigateToDashboardCommand.Execute();
                    break;

                case Resource.Id.menu_diagnostics:
                    this.ViewModel.NavigateToDiagnosticsPageCommand.Execute();
                    break;

                case Resource.Id.menu_signout:
                    this.ViewModel.SignOutCommand.Execute();
                    break;

                default:
                    if (item.GroupId == Resource.Id.interview_languages && !item.IsChecked)
                    {
                        Guid? translationId =
                            item.ItemId == Resource.Id.interview_language_original
                                ? null
                                : this.ViewModel.AvailableTranslations.FirstOrDefault(language=>language.Name == item.TitleFormatted.ToString())?.Id;

                        this.ViewModel.SwitchTranslationCommand.Execute(translationId);
                        this.ViewModel.ReloadInterviewCommand.Execute();
                    }
                    break;
            }
        }
    }
}