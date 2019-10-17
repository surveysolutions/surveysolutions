using System;
using System.Linq;
using System.Web.Mvc;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
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
        private readonly IRestServiceSettings restServiceSettings;

        public QuestionnairesController(
            IQuestionnaireStorage questionnaireStorage,
            IQuestionnaireBrowseViewFactory browseViewFactory,
            IWebInterviewConfigProvider webInterviewConfigProvider,
            IRestServiceSettings restServiceSettings)
        {
            this.questionnaireStorage = questionnaireStorage ?? throw new ArgumentNullException(nameof(questionnaireStorage));
            this.browseViewFactory = browseViewFactory ?? throw new ArgumentNullException(nameof(browseViewFactory));
            this.webInterviewConfigProvider = webInterviewConfigProvider ?? throw new ArgumentNullException(nameof(webInterviewConfigProvider));
            this.restServiceSettings = restServiceSettings ?? throw new ArgumentNullException(nameof(restServiceSettings));
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
            model.DesignerUrl = $"{this.restServiceSettings.Endpoint.TrimEnd('/')}/" +
                $"questionnaire/details/{questionnaire.QuestionnaireId:N}${questionnaire.Revision}";
            model.Comment = browseItem.Comment;

            FillStats(questionnaireIdentity, model);

            return View(model);
        }

        private void FillStats(QuestionnaireIdentity questionnaireIdentity, QuestionnaireDetailsModel model)
        {
            var document = this.questionnaireStorage.GetQuestionnaireDocument(questionnaireIdentity);

            foreach (var questionnaireEntry in document.Children.TreeToEnumerable(x => x.Children))
            {
                if (questionnaireEntry is IGroup group)
                {
                    if (group.GetParent().PublicKey == questionnaireIdentity.QuestionnaireId)
                    {
                       model.SectionsCount++;
                    }
                    else if (group.IsRoster)
                    {
                        model.RostersCount++;
                    }
                    else
                    {
                        model.SubSectionsCount++;
                    }
                }
                else
                {
                    if (questionnaireEntry is IQuestion question)
                    {
                        model.QuestionsCount++;
                        if (!string.IsNullOrEmpty(question.ConditionExpression))
                        {
                            model.QuestionsWithConditionsCount++;
                        }
                    }
                }
            }
        }
    }
}
