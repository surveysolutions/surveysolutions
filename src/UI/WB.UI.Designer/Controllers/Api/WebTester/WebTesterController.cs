using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Questionnaire.Api;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.UI.Designer.Api.WebTester;
using WB.UI.Designer.Services;

namespace WB.UI.Designer.Controllers.Api.WebTester
{
    [Route("api/webtester")]
    public class WebTesterController : ControllerBase
    {
        private readonly IQuestionnairePackageComposer questionnairePackageComposer;

        private readonly IQuestionnaireViewFactory questionnaireViewFactory;
        private readonly IAttachmentService attachmentService;
        private readonly IPlainStorageAccessor<TranslationInstance> translations;
        private readonly IWebTesterService webTesterService;


        public WebTesterController(
            IQuestionnairePackageComposer questionnairePackageComposer,
            IQuestionnaireViewFactory questionnaireViewFactory,
            IAttachmentService attachmentService,
            IPlainStorageAccessor<TranslationInstance> translations,
            IWebTesterService webTesterService)
        {
            this.questionnairePackageComposer = questionnairePackageComposer;
            this.questionnaireViewFactory = questionnaireViewFactory;
            this.attachmentService = attachmentService;
            this.translations = translations;
            this.webTesterService = webTesterService;
        }

        [Route("{token:Guid}/info")]
        [HttpGet]
        public IActionResult Info(string token)
        {
            var questionnaireId = this.webTesterService.GetQuestionnaire(token);
            if (questionnaireId == null)
            {
                return NotFound();
            }

            var questionnaireView = this.questionnaireViewFactory.Load(new QuestionnaireViewInputModel(questionnaireId.Value));
            if (questionnaireView == null)
            {
                return NotFound();
            }

            return Ok(new QuestionnaireLiteInfo
            {
                Id = questionnaireView.PublicKey,
                LastUpdateDate = questionnaireView.Source.LastEntryDate
            });
        }

        [Route("{token:Guid}/questionnaire")]
        [HttpGet]
        public IActionResult QuestionnaireAsync(string token)
        {
            var questionnaireId = this.webTesterService.GetQuestionnaire(token);
            if (questionnaireId == null) return NotFound();

            try
            {
                var composeQuestionnaire = this.questionnairePackageComposer.ComposeQuestionnaire(questionnaireId.Value);
                return Ok(composeQuestionnaire);
            }
            catch (ComposeException )
            {
                return StatusCode((int) HttpStatusCode.PreconditionFailed);
            }
        }

        [Route("{token:Guid}/attachment/{attachmentContentId}")]
        [HttpGet]
        public IActionResult AttachmentContentAsync(string token, string attachmentContentId)
        {
            var questionnaireId = this.webTesterService.GetQuestionnaire(token);
            if (questionnaireId == null) return NotFound();

            if (this.questionnaireViewFactory.Load(new QuestionnaireViewInputModel(questionnaireId.Value)) == null)
            {
                return NotFound();
            }

            var attachmentContent = this.attachmentService.GetContent(attachmentContentId);

            if (attachmentContent == null)
                return NotFound();

            return Ok(new AttachmentContent
            {
                ContentType = attachmentContent.ContentType,
                Content = attachmentContent.Content,
                Id = attachmentContent.ContentId,
                Size = attachmentContent.Size
            });
        }

        [Route("{token:Guid}/translations")]
        [HttpGet]
        public IActionResult TranslationsAsync(string token)
        {
            var questionnaireId = this.webTesterService.GetQuestionnaire(token);
            if (questionnaireId == null)
            {
                return NotFound();
            }

            var questionnaireView = this.questionnaireViewFactory.Load(new QuestionnaireViewInputModel(questionnaireId.Value));
            if (questionnaireView == null) return NotFound();

            var actualTranslations = questionnaireView.Source.Translations.Select(x => x.Id).ToList();

            return Ok(this.translations
                .Query(_ => _.Where(x => x.QuestionnaireId == questionnaireId && actualTranslations.Contains(x.TranslationId)).ToList())
                .Select(x => new TranslationDto
                {
                    Value = x.Value,
                    Type = x.Type,
                    TranslationId = x.TranslationId,
                    QuestionnaireEntityId = x.QuestionnaireEntityId,
                    TranslationIndex = x.TranslationIndex
                })
                .ToArray());
        }
    }
}
