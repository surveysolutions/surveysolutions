using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using MvvmCross.Droid.Views;
using MvvmCross.Platform;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Questionnaire.Translations;

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
            await this.BackwardCompatibilityAsync().ConfigureAwait(false);
            Mvx.Resolve<IViewModelNavigationService>().NavigateToLogin();
        }

        private async Task BackwardCompatibilityAsync()
        {
            await this.MoveCategoricalOptionsToPlainStorageIfNeeded();
        }

        [Obsolete("Released on the 1st of June. Version 5.9")]
        private async Task MoveCategoricalOptionsToPlainStorageIfNeeded()
        {
            var optionsRepository = Mvx.Resolve<IOptionsRepository>();

            var isMigrationNeeded = optionsRepository.IsEmpty(); //version less 5.9

            if (!isMigrationNeeded)
            {
                var optionsStorage = Mvx.Resolve<IAsyncPlainStorage<OptionView>>();
#pragma warning disable 472
                var isUpgradeNeeded = optionsStorage.Where(x => x.SortOrder == null).Any(); //version 5.10 upgrade
#pragma warning restore 472

                if (isUpgradeNeeded)
                {
                    isMigrationNeeded = true;
                    var optionViewRemover = Mvx.Resolve<IAsyncPlainStorage<OptionView>>();
                    await optionViewRemover.RemoveAllAsync();
                }
            }

            if (!isMigrationNeeded)
                return;

            var questionnaireViewRepository = Mvx.Resolve<IAsyncPlainStorage<QuestionnaireView>>();
            var questionnaireDocuments = Mvx.Resolve<IAsyncPlainStorage<QuestionnaireDocumentView>>();

            var questionnaires = await questionnaireViewRepository.LoadAllAsync();
            foreach (var questionnaireView in questionnaires)
            {
                var questionnaire = questionnaireDocuments.GetById(questionnaireView.Id);

                var questionsWithLongOptionsList = questionnaire.Document.Find<SingleQuestion>(
                    x => x.CascadeFromQuestionId.HasValue || (x.IsFilteredCombobox ?? false)).ToList();

                foreach (var question in questionsWithLongOptionsList)
                {
                    await optionsRepository.StoreOptionsForQuestionAsync(questionnaireView.Identity, question.PublicKey, question.Answers, new List<TranslationDto>());
                }
            }
        }
    }
}