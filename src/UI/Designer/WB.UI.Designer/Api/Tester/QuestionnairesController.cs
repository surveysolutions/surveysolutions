using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;
using WB.UI.Designer.Api.Attributes;
using WB.UI.Shared.Web.Membership;
using QuestionnaireListItem = WB.Core.SharedKernels.SurveySolutions.Api.Designer.QuestionnaireListItem;

namespace WB.UI.Designer.Api.Tester
{
    [ApiBasicAuth]
    [RoutePrefix("questionnaires")]
    public class QuestionnairesController : ApiController
    {
        private readonly IMembershipUserService userHelper;
        private readonly IQuestionnaireViewFactory questionnaireViewFactory;
        private readonly IQuestionnaireSharedPersonsFactory sharedPersonsViewFactory;
        private readonly IQuestionnaireVerifier questionnaireVerifier;
        private readonly IExpressionProcessorGenerator expressionProcessorGenerator;
        private readonly IQuestionnaireListViewFactory viewFactory;
        private readonly IDesignerEngineVersionService engineVersionService;

        public QuestionnairesController(IMembershipUserService userHelper,
            IQuestionnaireViewFactory questionnaireViewFactory,
            IQuestionnaireSharedPersonsFactory sharedPersonsViewFactory,
            IQuestionnaireVerifier questionnaireVerifier,
            IExpressionProcessorGenerator expressionProcessorGenerator,
            IQuestionnaireListViewFactory viewFactory, 
            IDesignerEngineVersionService engineVersionService)
        {
            this.userHelper = userHelper;
            this.questionnaireViewFactory = questionnaireViewFactory;
            this.sharedPersonsViewFactory = sharedPersonsViewFactory;
            this.questionnaireVerifier = questionnaireVerifier;
            this.expressionProcessorGenerator = expressionProcessorGenerator;
            this.viewFactory = viewFactory;
            this.engineVersionService = engineVersionService;
        }

        [HttpGet]
        [Route("{id:Guid}")]
        public Questionnaire Get(Guid id, int version)
        {
            if(version < ApiVersion.CurrentTesterProtocolVersion)
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.UpgradeRequired));

            var questionnaireView = this.questionnaireViewFactory.Load(new QuestionnaireViewInputModel(id));
            if (questionnaireView == null)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound));
            }

            if (!this.ValidateAccessPermissions(questionnaireView))
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
            }

            if (this.questionnaireVerifier.CheckForErrors(questionnaireView).Any())
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.PreconditionFailed));
            }

            var questionnaireContentVersion = this.engineVersionService.GetQuestionnaireContentVersion(questionnaireView.Source);

            string resultAssembly;
            try
            {
                GenerationResult generationResult = this.expressionProcessorGenerator.GenerateProcessorStateAssembly(
                    questionnaireView.Source,
                    questionnaireContentVersion, 
                    out resultAssembly);
                if(!generationResult.Success)
                    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.PreconditionFailed));
            }
            catch (Exception)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.PreconditionFailed));
            }

            var questionnaire = questionnaireView.Source.Clone();
            questionnaire.Macros = null;

            return new Questionnaire
            {
                Document = questionnaire,
                Assembly = resultAssembly
            };
        }

        [HttpGet]
        [Route("")] 
        public HttpResponseMessage Get(int version, [FromUri]int pageIndex = 1, [FromUri]int pageSize = 128)
        {
            if (version < ApiVersion.CurrentTesterProtocolVersion)
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.UpgradeRequired));

            var userId = this.userHelper.WebUser.UserId;
            var isAdmin = this.userHelper.WebUser.IsAdmin;

            var questionnaireViews = this.viewFactory.GetUserQuestionnaires(userId, isAdmin, pageIndex, pageSize);

            var questionnaires = questionnaireViews.Select(questionnaire => new QuestionnaireListItem
            {
                Id = questionnaire.QuestionnaireId,
                Title = questionnaire.Title,
                LastEntryDate = questionnaire.LastEntryDate,
                Owner = questionnaire.CreatorName,
                IsPublic = questionnaire.IsPublic || isAdmin,
                IsShared = questionnaire.SharedPersons.Any(sharedPerson => sharedPerson.Id == userId)
            });

            var response = this.Request.CreateResponse(questionnaires);
            response.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true
            };
            return response;
        }

        private bool ValidateAccessPermissions(QuestionnaireView questionnaireView)
        {
            if (questionnaireView.IsPublic || questionnaireView.CreatedBy == this.userHelper.WebUser.UserId || this.userHelper.WebUser.IsAdmin)
                return true;


            return questionnaireView.SharedPersons.Any(x => x.Id == this.userHelper.WebUser.UserId);
        }
    }
}