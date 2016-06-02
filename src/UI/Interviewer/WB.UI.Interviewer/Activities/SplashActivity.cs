using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using MvvmCross.Droid.Views;
using MvvmCross.Platform;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.UI.Interviewer.Activities
{
    [Activity(NoHistory = true, MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait, Theme = "@style/AppTheme")]
    public class SplashActivity : MvxSplashScreenActivity
    {
        public SplashActivity() : base(Resource.Layout.splash)
        {
        }

        protected override async void TriggerFirstNavigate()
        {
            await this.BackwardCompatibilityAsync();
            await Mvx.Resolve<IViewModelNavigationService>().NavigateToLoginAsync();
        }

        private async Task BackwardCompatibilityAsync()
        {
            var settingsStorage = Mvx.Resolve<IAsyncPlainStorage<ApplicationSettingsView>>();
            var settings = settingsStorage.FirstOrDefault();
            if (settings != null)
            {
                await this.MigrateCategoricalOptionsAndSetReadSideVersionTo1(settings, settingsStorage);
                return;
            }

            await MoveCategoricalOptionsToPlainStorage();
        }

        private async Task MigrateCategoricalOptionsAndSetReadSideVersionTo1(ApplicationSettingsView settings, IAsyncPlainStorage<ApplicationSettingsView> settingsStorage)
        {
            var isMigrationNeeded = !settings.ReadSideVersion.HasValue;

            if (!isMigrationNeeded)
                return;

            await this.MoveCategoricalOptionsToPlainStorage();
            settings.ReadSideVersion = 1;
            await settingsStorage.StoreAsync(settings);
        }

        private async Task MoveCategoricalOptionsToPlainStorage()
        {
            var questionnaireViewRepository = Mvx.Resolve<IAsyncPlainStorage<QuestionnaireView>>();
            var questionnaireDocuments = Mvx.Resolve<IAsyncPlainStorage<QuestionnaireDocumentView>>();
            var optionsRepository = Mvx.Resolve<IOptionsRepository>();

            var questionnaires = await questionnaireViewRepository.LoadAllAsync();
            foreach (var questionnaireView in questionnaires)
            {
                var questionnaire = questionnaireDocuments.GetById(questionnaireView.Id);
                await optionsRepository.StoreQuestionOptionsForQuestionnaireAsync(questionnaireView.Identity, questionnaire.Document);
            }
        }
    }
}