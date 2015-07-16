using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;
using WB.UI.Designer.Api.Attributes;
using WB.UI.Shared.Web.Membership;
using QuestionnaireListItem = WB.Core.SharedKernels.SurveySolutions.Api.Designer.QuestionnaireListItem;

namespace WB.UI.Designer.Api
{
    [ApiBasicAuth]
    [RoutePrefix("api/v8/questionnaires")]
    public class QuestionnairesController : ApiController
    {
        private readonly Version ApiVersion = new Version(8, 0, 0, 0);

        private readonly IMembershipUserService userHelper;
        private readonly IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory;
        private readonly IViewFactory<QuestionnaireSharedPersonsInputModel, QuestionnaireSharedPersons> sharedPersonsViewFactory;
        private readonly IQuestionnaireVerifier questionnaireVerifier;
        private readonly IExpressionProcessorGenerator expressionProcessorGenerator;
        private readonly IQuestionnaireListViewFactory viewFactory;
        private readonly IExpressionsEngineVersionService expressionsEngineVersionService;
        public QuestionnairesController(IMembershipUserService userHelper,
            IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory,
            IViewFactory<QuestionnaireSharedPersonsInputModel, QuestionnaireSharedPersons> sharedPersonsViewFactory,
            IQuestionnaireVerifier questionnaireVerifier,
            IExpressionProcessorGenerator expressionProcessorGenerator,
            IQuestionnaireListViewFactory viewFactory, IExpressionsEngineVersionService expressionsEngineVersionService)
        {
            this.userHelper = userHelper;
            this.questionnaireViewFactory = questionnaireViewFactory;
            this.sharedPersonsViewFactory = sharedPersonsViewFactory;
            this.questionnaireVerifier = questionnaireVerifier;
            this.expressionProcessorGenerator = expressionProcessorGenerator;
            this.viewFactory = viewFactory;
            this.expressionsEngineVersionService = expressionsEngineVersionService;
        }

        [Route("~/api/v8/login")]
        [HttpGet]
        public void Login()
        {
        }

        [Route("{id:Guid}")]
        public Questionnaire Get(Guid id)
        {
            var questionnaireView = questionnaireViewFactory.Load(new QuestionnaireViewInputModel(id));
            if (questionnaireView == null)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound));
            }

            if (!this.ValidateAccessPermissions(questionnaireView))
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
            }

            if (this.questionnaireVerifier.Verify(questionnaireView.Source).Any())
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.PreconditionFailed));
            }

            string resultAssembly;
            try
            {
                GenerationResult generationResult = this.expressionProcessorGenerator.GenerateProcessorStateAssembly(
                    questionnaireView.Source,
                    ApiVersion, 
                    out resultAssembly);
                if(!generationResult.Success)
                    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.PreconditionFailed));
            }
            catch (Exception)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.PreconditionFailed));
            }

            return new Questionnaire()
            {
                Document = questionnaireView.Source,
                Assembly = resultAssembly
            };
        }

        [Route("")]
        public IEnumerable<QuestionnaireListItem> Get([FromUri]int pageIndex = 1, [FromUri]int pageSize = 128, [FromUri]string sortBy = "", [FromUri]string filter = "", [FromUri]bool isPublic = false)
        {
            var questionnaireListView = this.viewFactory.Load(new QuestionnaireListInputModel
            {
                ViewerId = this.userHelper.WebUser.UserId,
                IsAdminMode = this.userHelper.WebUser.IsAdmin,
                IsPublic = isPublic,
                Page = pageIndex,
                PageSize = pageSize,
                Order = string.IsNullOrEmpty(sortBy) ? "LastEntryDate DESC" : sortBy,
                Filter = filter
            });

            return questionnaireListView.Items.Select(questionnaire => new QuestionnaireListItem()
            {
                Id = questionnaire.PublicId.FormatGuid(),
                Title = questionnaire.Title,
                LastEntryDate = questionnaire.LastEntryDate
            });
        }

        private bool ValidateAccessPermissions(QuestionnaireView questionnaireView)
        {
            if (questionnaireView.IsPublic || questionnaireView.CreatedBy == this.userHelper.WebUser.UserId)
                return true;

            QuestionnaireSharedPersons questionnaireSharedPersons =
                this.sharedPersonsViewFactory.Load(new QuestionnaireSharedPersonsInputModel() { QuestionnaireId = questionnaireView.PublicKey });

            return (questionnaireSharedPersons != null) && questionnaireSharedPersons.SharedPersons.Any(x => x.Id == this.userHelper.WebUser.UserId);
        }
    }
}