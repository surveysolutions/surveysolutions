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
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.GenericSubdomains.Portable.Services;
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

        protected override void TriggerFirstNavigate()
        {
            this.BackwardCompatibility();
            Mvx.Resolve<IViewModelNavigationService>().NavigateToLogin();
        }

        private void BackwardCompatibility()
        {
            this.MoveCategoricalOptionsToPlainStorageIfNeeded();
        }

        [Obsolete("Released on the 1st of June. Version 5.9")]
        private void MoveCategoricalOptionsToPlainStorageIfNeeded()
        {
            var optionsRepository = Mvx.Resolve<IOptionsRepository>();

            var isMigrationNeeded = optionsRepository.IsEmpty(); //version less 5.9

            if (!isMigrationNeeded)
            {
                var optionsStorage = Mvx.Resolve<IPlainStorage<OptionView>>();
#pragma warning disable 472
                var isUpgradeNeeded = optionsStorage.Where(x => x.SortOrder == null).Any(); //version 5.10 upgrade
#pragma warning restore 472

                if (isUpgradeNeeded)
                {
                    isMigrationNeeded = true;
                    var optionViewRemover = Mvx.Resolve<IPlainStorage<OptionView>>();
                    optionViewRemover.RemoveAll();
                }
            }

            MigrateDashboardToOfficialSqliteRelease();

            if (!isMigrationNeeded)
                return;

            var questionnaireViewRepository = Mvx.Resolve<IPlainStorage<QuestionnaireView>>();
            var questionnaireDocuments = Mvx.Resolve<IPlainStorage<QuestionnaireDocumentView>>();

            var questionnaires = questionnaireViewRepository.LoadAll();
            foreach (var questionnaireView in questionnaires)
            {
                var questionnaire = questionnaireDocuments.GetById(questionnaireView.Id);

                var questionsWithLongOptionsList = questionnaire.QuestionnaireDocument.Find<SingleQuestion>(
                    x => x.CascadeFromQuestionId.HasValue || (x.IsFilteredCombobox ?? false)).ToList();

                foreach (var question in questionsWithLongOptionsList)
                {
                    optionsRepository.StoreOptionsForQuestion(questionnaireView.GetIdentity(), question.PublicKey, question.Answers, new List<TranslationDto>());
                }
            }
        }

        private void MigrateDashboardToOfficialSqliteRelease()
        {
            var dashboardItems = Mvx.Resolve<IPlainStorage<InterviewView>>();
            var prefilledQuestions = Mvx.Resolve<IPlainStorage<PrefilledQuestionView>>();
            var serializer = Mvx.Resolve<IJsonAllTypesSerializer>();

            var allInterviews = dashboardItems.Where(x => x.AnswersOnPrefilledQuestions != null || x.GpsLocation != null).ToList();
            foreach (InterviewView interviewToMigrate in allInterviews)
            {
                if (interviewToMigrate.AnswersOnPrefilledQuestions != null)
                {
                    InterviewAnswerOnPrefilledQuestionView[] oldAnswers =
                        serializer.Deserialize<InterviewAnswerOnPrefilledQuestionView[]>(
                            interviewToMigrate.AnswersOnPrefilledQuestions);

                    var newPrefilled = oldAnswers.Select(x => new PrefilledQuestionView
                    {
                        Answer = x.Answer,
                        Id = $"{interviewToMigrate.InterviewId:N}${x.QuestionId:N}",
                        InterviewId = interviewToMigrate.InterviewId,
                        QuestionId = x.QuestionId,
                        QuestionText = x.QuestionText
                    });

                    prefilledQuestions.Store(newPrefilled);
                    interviewToMigrate.AnswersOnPrefilledQuestions = null;
                    dashboardItems.Store(interviewToMigrate);
                }

                if (interviewToMigrate.GpsLocation != null)
                {
                    var interviewGpsLocationView = serializer.Deserialize<InterviewGpsLocationView>(interviewToMigrate.GpsLocation);
                    if (interviewGpsLocationView != null)
                    {
                        interviewToMigrate.LocationQuestionId = interviewGpsLocationView.PrefilledQuestionId;
                        interviewToMigrate.LocationLongitude = interviewGpsLocationView.Coordinates?.Longitude;
                        interviewToMigrate.LocationLatitude = interviewGpsLocationView.Coordinates?.Latitude;

                        dashboardItems.Store(interviewToMigrate);
                    }
                }
            }
        }
    }
}