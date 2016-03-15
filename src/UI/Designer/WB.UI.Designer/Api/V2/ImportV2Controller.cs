using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.UI.Designer.Api.Attributes;
using WB.UI.Designer.Code;
using WB.UI.Shared.Web.Membership;

namespace WB.UI.Designer.Api
{
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
            IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory,
            IViewFactory<QuestionnaireSharedPersonsInputModel, QuestionnaireSharedPersons> sharedPersonsViewFactory,
            IQuestionnaireVerifier questionnaireVerifier,
            IExpressionProcessorGenerator expressionProcessorGenerator,
            IQuestionnaireHelper questionnaireHelper,
            IDesignerEngineVersionService engineVersionService,
            ISerializer serializer,
            IAttachmentService attachmentService)
            : base(userHelper, viewFactory, questionnaireViewFactory, sharedPersonsViewFactory,
                questionnaireVerifier, expressionProcessorGenerator, questionnaireHelper, engineVersionService)
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
        

        [HttpGet]
        [Route("QuestionnaireList")]
        public override QuestionnaireListCommunicationPackage QuestionnaireList() => base.QuestionnaireList();
        

        [HttpPost]
        [Route("Questionnaire")]
        public QuestionnaireCommunicationPackage Questionnaire(DownloadQuestionnaireRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var questionnaireView = this.GetQuestionnaireViewOrThrow(request);

            this.CheckInvariantsAndThrowIfInvalid(request, questionnaireView);

            var questionnaireContentVersion = this.engineVersionService.GetQuestionnaireContentVersion(questionnaireView.Source);

            var resultAssembly = this.GetQuestionnaireAssemblyOrThrow(questionnaireView, questionnaireContentVersion);

            var questionnaire = questionnaireView.Source.Clone();
            questionnaire.Macros = null;
            questionnaire.LookupTables = null;
            questionnaire.SharedPersons = null;

            var attachmentMeta = attachmentService.GetBriefAttachmentsMetaForQuestionnaire(request.QuestionnaireId)
                .Where(x => questionnaire.Attachments.Any(a => a.AttachmentId == x.AttachmentId))
                .ToArray();

            return new QuestionnaireCommunicationPackage
            {
                Questionnaire = this.zipUtils.CompressString(this.serializer.Serialize(questionnaire, SerializationBinderSettings.OldToNew)), // use binder to serialize to the old namespaces and assembly
                QuestionnaireAssembly = resultAssembly,
                QuestionnaireContentVersion = questionnaireContentVersion.Major,
                Attachments = attachmentMeta
            };
        }

        [HttpGet]
        [Route("attachments/{id:Guid}")]
        public HttpResponseMessage Attachment(Guid id)
        {
            var attachment = this.attachmentService.GetAttachment(id);

            if (attachment == null) return Request.CreateResponse(HttpStatusCode.NotFound);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(new MemoryStream(attachment.Content))
            };

            response.Content.Headers.ContentType = new MediaTypeHeaderValue(attachment.ContentType);
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = attachment.FileName
            };
            response.Headers.ETag = new EntityTagHeaderValue("\"" + attachment.AttachmentContentId + "\"");

            return response;
        }
    }
}