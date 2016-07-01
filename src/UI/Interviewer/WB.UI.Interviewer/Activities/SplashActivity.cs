using System;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using MvvmCross.Droid.Views;
using MvvmCross.Platform;
using SQLite.Net.Interop;
using WB.Core.BoundedContexts.Interviewer.Implementation.Storage;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.SharedKernels.Enumerator;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.UI.Interviewer.Migrations;

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
                    var optionViewRemover = Mvx.Resolve<IAsyncPlainStorageRemover<OptionView>>();
                    await optionViewRemover.DeleteAllAsync();
                }
            }

            if (isMigrationNeeded)
            {
                var questionnaireViewRepository = Mvx.Resolve<IAsyncPlainStorage<QuestionnaireView>>();
                var questionnaireDocuments = Mvx.Resolve<IAsyncPlainStorage<QuestionnaireDocumentView>>();

                var questionnaires = await questionnaireViewRepository.LoadAllAsync();
                foreach (var questionnaireView in questionnaires)
                {
                    var questionnaire = questionnaireDocuments.GetById(questionnaireView.Id);
                    await
                        optionsRepository.StoreQuestionOptionsForQuestionnaireAsync(questionnaireView.Identity,
                            questionnaire.Document);
                }

            }

            var eventStoreMigrator = new EventStoreMigrator(Mvx.Resolve<ISQLitePlatform>(),
                Mvx.Resolve<SqliteSettings>(), Mvx.Resolve<IEnumeratorSettings>(), Mvx.Resolve<IAsyncPlainStorage<InterviewView>>());
            
            if (eventStoreMigrator.IsMigrationShouldBeDone())
            {
                var interviewerEventStorage = Mvx.Resolve<IInterviewerEventStorage>();
                await Task.Run(() => eventStoreMigrator.Migrate(interviewerEventStorage)).ConfigureAwait(false);
            }
        }
    }
}