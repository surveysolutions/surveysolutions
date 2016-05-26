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
            var readSideVersion = 1;
            var settingsStorage = Mvx.Resolve<IAsyncPlainStorage<ApplicationSettingsView>>();
            var settings = settingsStorage.FirstOrDefault();
            if (settings != null)
            {
                if (!settings.ReadSideVersion.HasValue)
                {
                    await MoveCategoricalOptionsToPlainStorage();
                    settings.ReadSideVersion = readSideVersion;
                    await settingsStorage.StoreAsync(settings);
                }
                return;
            }

            await MoveCategoricalOptionsToPlainStorage();
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