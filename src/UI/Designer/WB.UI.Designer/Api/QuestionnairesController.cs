using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;
using WB.UI.Designer.Api.Attributes;
using WB.UI.Shared.Web.Membership;
using QuestionnaireListItem = WB.Core.SharedKernels.SurveySolutions.Api.Designer.QuestionnaireListItem;

namespace WB.UI.Designer.Api
{
    [ApiBasicAuth]
    [RoutePrefix("api/v15/questionnaires")]
    public class QuestionnairesController : ApiController
    {
        //temporary fix
        //api version should not be used as version for compilation
        internal static readonly Version ApiVersion = new Version(13, 0, 0);

        private readonly IMembershipUserService userHelper;
        private readonly IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory;
        private readonly IViewFactory<QuestionnaireSharedPersonsInputModel, QuestionnaireSharedPersons> sharedPersonsViewFactory;
        private readonly IQuestionnaireVerifier questionnaireVerifier;
        private readonly IExpressionProcessorGenerator expressionProcessorGenerator;
        private readonly IQuestionnaireListViewFactory viewFactory;
        private readonly IAttachmentService attachmentService;

        public QuestionnairesController(IMembershipUserService userHelper,
            IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory,
            IViewFactory<QuestionnaireSharedPersonsInputModel, QuestionnaireSharedPersons> sharedPersonsViewFactory,
            IQuestionnaireVerifier questionnaireVerifier,
            IExpressionProcessorGenerator expressionProcessorGenerator,
            IQuestionnaireListViewFactory viewFactory, 
            IAttachmentService attachmentService)
        {
            this.userHelper = userHelper;
            this.questionnaireViewFactory = questionnaireViewFactory;
            this.sharedPersonsViewFactory = sharedPersonsViewFactory;
            this.questionnaireVerifier = questionnaireVerifier;
            this.expressionProcessorGenerator = expressionProcessorGenerator;
            this.viewFactory = viewFactory;
            this.attachmentService = attachmentService;
        }

        [Route("~/api/v15/login")]
        [HttpGet]
        public void Login()
        {
        }

        [Route("~/api/v15/attachment/{id:Guid}")]
        [HttpGet]
        public HttpResponseMessage Attachment(Guid id)
        {
            var questionnaireAttachment = this.attachmentService.GetAttachment(id);

            if (questionnaireAttachment?.Content == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(questionnaireAttachment.Content)
            };

            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            return response;
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

            if (this.questionnaireVerifier.CheckForErrors(questionnaireView.Source).Any())
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

            var questionnaire = questionnaireView.Source.Clone();
            questionnaire.Macros = null;

            var attachmentMeta = attachmentService.GetBriefAttachmentsMetaForQuestionnaire(id)
                .Where(x => questionnaire.Attachments.Any(a => a.AttachmentId == x.AttachmentId))
                .ToArray();

            return new Questionnaire
            {
                Document = questionnaire,
                Assembly = resultAssembly,
                AttachmentsMeta = attachmentMeta
            };
        }

        [Route("")]
        public HttpResponseMessage Get([FromUri]int pageIndex = 1, [FromUri]int pageSize = 128)
        {
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
                IsShared = questionnaire.SharedPersons.Any(sharedPerson => sharedPerson == userId)
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

            QuestionnaireSharedPersons questionnaireSharedPersons =
                this.sharedPersonsViewFactory.Load(new QuestionnaireSharedPersonsInputModel() { QuestionnaireId = questionnaireView.PublicKey });

            return (questionnaireSharedPersons != null) && questionnaireSharedPersons.SharedPersons.Any(x => x.Id == this.userHelper.WebUser.UserId);
        }
    }
}