using System;
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
            await this.MoveCategoricalOptionsToPlainStorageIfNeeded();
        }

        [Obsolete("Released on the 1st of June. Version 5.9")]
        private async Task MoveCategoricalOptionsToPlainStorageIfNeeded()
        {
            var optionsRepository = Mvx.Resolve<IOptionsRepository>();

            var isMigrationNeeded = optionsRepository.OptionsCount() == 0;

            if (!isMigrationNeeded)
                return;

            var questionnaireViewRepository = Mvx.Resolve<IAsyncPlainStorage<QuestionnaireView>>();
            var questionnaireDocuments = Mvx.Resolve<IAsyncPlainStorage<QuestionnaireDocumentView>>();

            var questionnaires = await questionnaireViewRepository.LoadAllAsync();
            foreach (var questionnaireView in questionnaires)
            {
                var questionnaire = questionnaireDocuments.GetById(questionnaireView.Id);
                await optionsRepository.StoreQuestionOptionsForQuestionnaireAsync(questionnaireView.Identity, questionnaire.Document);
            }
        }
    }
}