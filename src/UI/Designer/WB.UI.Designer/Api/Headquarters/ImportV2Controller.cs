using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.UI.Designer.Api.Attributes;
using WB.UI.Shared.Web.Membership;

namespace WB.UI.Designer.Api.Headquarters
{
    [Obsolete("Since v5.11")]
    [ApiBasicAuth]
    [RoutePrefix("api/v2/import")]
    public class ImportV2Controller : ImportControllerBase
    {
        private readonly IStringCompressor zipUtils;
        private readonly ISerializer serializer;
        private readonly IAttachmentService attachmentService;

        public ImportV2Controller(
            IStringCompressor zipUtils,
            IMembershipUserService userHelper,
            IQuestionnaireListViewFactory viewFactory,
            IQuestionnaireViewFactory questionnaireViewFactory,
            IQuestionnaireVerifier questionnaireVerifier,
            IExpressionProcessorGenerator expressionProcessorGenerator,
            IDesignerEngineVersionService engineVersionService,
            ISerializer serializer,
            IAttachmentService attachmentService)
            : base(userHelper, viewFactory, questionnaireViewFactory, 
                questionnaireVerifier, expressionProcessorGenerator, engineVersionService)
        {
            this.zipUtils = zipUtils;
            this.serializer = serializer;
            this.attachmentService = attachmentService;
        }

        [HttpGet]
        [Route("login")]
        public override void Login() => base.Login(); 

        [HttpPost]
        [Route("PagedQuestionnaireList")]
        public override PagedQuestionnaireCommunicationPackage PagedQuestionnaireList(QuestionnaireListRequest request) => base.PagedQuestionnaireList(request);

        [HttpPost]
        [Route("Questionnaire")]
        public QuestionnaireCommunicationPackage Questionnaire(DownloadQuestionnaireRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var questionnaireView = this.GetQuestionnaireViewOrThrow(request);

            this.CheckInvariantsAndThrowIfInvalid(request.SupportedVersion.Major, questionnaireView);

            var questionnaireContentVersion = this.engineVersionService.GetQuestionnaireContentVersion(questionnaireView.Source);

            var resultAssembly = this.GetQuestionnaireAssemblyOrThrow(questionnaireView, questionnaireContentVersion);

            var questionnaire = questionnaireView.Source.Clone();
            questionnaire.Macros = null;
            questionnaire.LookupTables = null;

            return new QuestionnaireCommunicationPackage
            {
                Questionnaire = this.zipUtils.CompressString(this.serializer.Serialize(questionnaire)), // use binder to serialize to the old namespaces and assembly
                QuestionnaireAssembly = resultAssembly,
                QuestionnaireContentVersion = questionnaireContentVersion
            };
        }

        [HttpGet]
        [Route("attachments/{id}")]
        public HttpResponseMessage AttachmentContent(string id)
        {
            var attachment = this.attachmentService.GetContent(id);

            if (attachment == null) return this.Request.CreateResponse(HttpStatusCode.NotFound);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(attachment.Content)
            };

            response.Content.Headers.ContentType = new MediaTypeHeaderValue(attachment.ContentType);
            response.Headers.ETag = new EntityTagHeaderValue("\"" + attachment.ContentId + "\"");

            return response;
        }
    }
}