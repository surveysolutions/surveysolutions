using Android.App;
using Android.Support.Design.Widget;
using Android.Views;
using Humanizer;
using Humanizer.Localisation;
using MvvmCross.Platform;
using MvvmCross.Plugins.Messenger;
using WB.Core.BoundedContexts.Tester.Properties;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.Enumerator;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.UI.Shared.Enumerator.Activities;
using WB.UI.Tester.Infrastructure.Internals.Settings;

namespace WB.UI.Tester.Activities
{
    [Activity(Label = "", Theme = "@style/BlueAppTheme", HardwareAccelerated = true,
        WindowSoftInputMode = SoftInput.StateAlwaysHidden | SoftInput.AdjustPan,
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class InterviewActivity : BaseInterviewActivity<InterviewViewModel>
    {
        private MvxSubscriptionToken answerAcceptedSubsribtion;
        protected override int LanguagesMenuGroupId => Resource.Id.interview_languages;
        protected override int OriginalLanguageMenuItemId => Resource.Id.interview_language_original;
        protected override int LanguagesMenuItemId => Resource.Id.interview_language;
        protected override int MenuId => Resource.Menu.interview;
        private bool showAnswerAcceptedToast = true;

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

            SetupAnswerTimeMeasurement();
        }

        private void SetupAnswerTimeMeasurement()
        {
            var settings = Mvx.Resolve<TesterSettings>();

            if (settings.ShowAnswerTime && answerAcceptedSubsribtion == null)
            {
                var mvxMessenger = ServiceLocator.Current.GetInstance<IMvxMessenger>();
                answerAcceptedSubsribtion = mvxMessenger.Subscribe<AnswerAcceptedMessage>(msg =>
                {
                    if (showAnswerAcceptedToast)
                    {
                        var message = string.Format(TesterUIResources.AnswerRecordedMsg,
                            msg.Elapsed.Humanize(maxUnit: TimeUnit.Minute));

                        Snackbar.Make(this.FindViewById(Resource.Id.drawer_layout),
                                message,
                                Snackbar.LengthIndefinite)
                            .SetAction(TesterUIResources.AnswerRecordedMsgDismiss, view => { })
                            .Show(); // Don’t forget to show!
                    }
                });
            }
        }

        protected override void OnPause()
        {
            base.OnPause();
            this.answerAcceptedSubsribtion?.Dispose();
            this.answerAcceptedSubsribtion = null;
        }
    }
}