using System;
using System.Linq;
using System.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.UI.Headquarters.Models;

namespace WB.UI.Headquarters.Controllers
{
    [LimitsFilter, Authorize(Roles = "Administrator, Headquarter, Supervisor")]
    public class QuestionnairesController : Controller
    {
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IQuestionnaireBrowseViewFactory browseViewFactory;
        private readonly IWebInterviewConfigProvider webInterviewConfigProvider;

        public QuestionnairesController(IQuestionnaireStorage questionnaireStorage,
            IQuestionnaireBrowseViewFactory browseViewFactory, 
            IWebInterviewConfigProvider webInterviewConfigProvider)
        {
            this.questionnaireStorage = questionnaireStorage ?? throw new ArgumentNullException(nameof(questionnaireStorage));
            this.browseViewFactory = browseViewFactory ?? throw new ArgumentNullException(nameof(browseViewFactory));
            this.webInterviewConfigProvider = webInterviewConfigProvider ?? throw new ArgumentNullException(nameof(webInterviewConfigProvider));
        }

        [OutputCache(Duration = 1200)]
        public ActionResult Details(string id)
        {
            var questionnaireIdentity = QuestionnaireIdentity.Parse(id);
            var questionnaire = this.questionnaireStorage.GetQuestionnaire(questionnaireIdentity, null);
            var model = new QuestionnaireDetailsModel
            {
                Title = questionnaire.Title, 
                Version = questionnaire.Version
            };

            var browseItem = browseViewFactory.GetById(questionnaireIdentity);
            model.ImportDateUtc = browseItem.ImportDate.GetValueOrDefault();
            model.LastEntryDateUtc = browseItem.LastEntryDate;
            model.CreationDateUtc = browseItem.CreationDate;
            model.WebMode = this.webInterviewConfigProvider.Get(questionnaireIdentity).Started;
            model.AudioAudit = browseItem.IsAudioRecordingEnabled;
            model.SectionsCount = questionnaire.GetAllSections().Count();
            model.SubSectionsCount = questionnaire.GetAllGroups().Count;
            model.RostersCount = questionnaire.GetAllRosters().Count;
            var questions = questionnaire.GetAllQuestions();
            model.QuestionsCount = questions.Count;
            int withConditionsCount = 0;
            foreach (var question in questions)
            {
                if(!string.IsNullOrEmpty(questionnaire.GetCustomEnablementConditionForQuestion(question)))
                {
                    withConditionsCount++;
                }
            }

            model.QuestionsWithConditionsCount = withConditionsCount;

            return View(model);
        }
    }
}
