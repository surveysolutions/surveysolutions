using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.UI.Designer.Code;
using WB.UI.Designer.Code.Attributes;
using WB.Core.BoundedContexts.Designer;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.UI.Designer.Resources;

namespace WB.UI.Designer.Controllers.Api.Headquarters
{
    [Obsolete("Since v5.11")]
    [Authorize]
    [Route("api/v2/import")]
    public class ImportV2Controller : ImportControllerBase
    {
        private readonly IStringCompressor zipUtils;
        private readonly IQuestionnaireViewFactory questionnaireViewFactory;
        private readonly ISerializer serializer;
        private readonly IAttachmentService attachmentService;
        private readonly IExpressionProcessorGenerator expressionProcessorGenerator;

        public ImportV2Controller(
            IStringCompressor zipUtils,
            IQuestionnaireListViewFactory viewFactory,
            IQuestionnaireViewFactory questionnaireViewFactory,
            IQuestionnaireVerifier questionnaireVerifier,
            IExpressionProcessorGenerator expressionProcessorGenerator,
            IDesignerEngineVersionService engineVersionService,
            ISerializer serializer,
            IAttachmentService attachmentService)
            : base(viewFactory, questionnaireVerifier, engineVersionService)
        {
            this.zipUtils = zipUtils;
            this.questionnaireViewFactory = questionnaireViewFactory;
            this.expressionProcessorGenerator = expressionProcessorGenerator;
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
        public IActionResult Questionnaire(DownloadQuestionnaireRequest request)
        {
            if (request?.SupportedVersion == null) throw new ArgumentNullException(nameof(request));
            
            var questionnaireView1 = this.questionnaireViewFactory.Load(new QuestionnaireViewInputModel(request.QuestionnaireId));
            if (questionnaireView1 == null)
            {
                return this.Error(StatusCodes.Status404NotFound,
                    string.Format(ErrorMessages.TemplateNotFound, request.QuestionnaireId));
            }

            if (!this.ValidateAccessPermissions(questionnaireView1))
            {
                return this.Error(StatusCodes.Status403Forbidden, ErrorMessages.User_Not_authorized);
            }

            var questionnaireView = questionnaireView1;

            var checkResult = this.CheckInvariants(request.SupportedVersion.Major, questionnaireView);
            if (checkResult != null)
                return checkResult;

            var questionnaireContentVersion = this.engineVersionService.GetQuestionnaireContentVersion(questionnaireView.Source);

            var resultAssembly = this.GetQuestionnaireAssembly(questionnaireView, questionnaireContentVersion);

            if (string.IsNullOrEmpty(resultAssembly))
            {
                var message = string.Format(
                    ErrorMessages.YourQuestionnaire_0_ContainsNewFunctionalityWhichIsNotSupportedByYourInstallationPleaseUpdate,
                    questionnaireView.Title, "Unknown");
                return this.Error(StatusCodes.Status426UpgradeRequired, message);
            }

            var questionnaire = questionnaireView.Source.Clone();
            questionnaire.Macros = new Dictionary<Guid, Macro>();
            questionnaire.LookupTables = new Dictionary<Guid, LookupTable>();
            questionnaire.IsUsingExpressionStorage = questionnaireContentVersion > 19;

            var questionnaireCommunicationPackage = new QuestionnaireCommunicationPackage
            (
                questionnaire : this.zipUtils.CompressString(this.serializer.Serialize(questionnaire)), // use binder to serialize to the old namespaces and assembly
                questionnaireAssembly : resultAssembly,
                questionnaireContentVersion : questionnaireContentVersion
            );
            return Ok(questionnaireCommunicationPackage);
        }

        private string GetQuestionnaireAssembly(QuestionnaireView questionnaireView, int questionnaireContentVersion)
        {
            string resultAssembly;
            try
            {
                this.expressionProcessorGenerator.GenerateProcessorStateAssembly(
                    questionnaireView.Source, questionnaireContentVersion, out resultAssembly);
            }
            catch (Exception)
            {
                resultAssembly = string.Empty;
            }

            return resultAssembly;
        }

        [HttpGet]
        [Route("attachments/{id}")]
        public IActionResult AttachmentContent(string id)
        {
            var attachment = this.attachmentService.GetContent(id);

            if (attachment == null) return StatusCode(StatusCodes.Status404NotFound);

            return File(attachment.Content, attachment.ContentType, null,
                new Microsoft.Net.Http.Headers.EntityTagHeaderValue("\"" + attachment.ContentId + "\""));
        }

        private bool ValidateAccessPermissions(QuestionnaireView questionnaireView)
        {
            if (questionnaireView.CreatedBy == this.User.GetId())
                return true;


            return questionnaireView.SharedPersons.Any(x => x.UserId == this.User.GetId());
        }
    }
}
