using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Questionnaire.Api;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;
using WB.UI.Designer.Services;

namespace WB.UI.Designer.Api.WebTester
{
    [RoutePrefix("api/webtester")]
    public class WebTesterController : ApiController
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
        public Task<QuestionnaireLiteInfo> InfoAsync(string token)
        {
            var questionnaireId = this.webTesterService.GetQuestionnaire(token) ?? throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound));

            var questionnaireView = this.questionnaireViewFactory.Load(new QuestionnaireViewInputModel(questionnaireId))
                ?? throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound));

            return Task.FromResult(new QuestionnaireLiteInfo
            {
                Id = questionnaireView.PublicKey,
                LastUpdateDate = questionnaireView.Source.LastEntryDate
            });
        }

        [Route("{token:Guid}/questionnaire")]
        [HttpGet]
        public Questionnaire QuestionnaireAsync(string token)
        {
            var questionnaireId = this.webTesterService.GetQuestionnaire(token) 
                                  ?? throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound));

            return this.questionnairePackageComposer.ComposeQuestionnaire(questionnaireId);
        }

        [Route("{token:Guid}/attachment/{attachmentContentId}")]
        [HttpGet]
        public Task<AttachmentContent> AttachmentContentAsync(string token, string attachmentContentId)
        {
            var questionnaireId = this.webTesterService.GetQuestionnaire(token)
                                  ?? throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound));

            if (this.questionnaireViewFactory.Load(new QuestionnaireViewInputModel(questionnaireId)) == null)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound));
            }

            var attachmentContent = this.attachmentService.GetContent(attachmentContentId);
            
            if (attachmentContent == null) throw new HttpResponseException(HttpStatusCode.NotFound);

            return Task.FromResult(new AttachmentContent
            {
                ContentType = attachmentContent.ContentType,
                Content = attachmentContent.Content,
                Id = attachmentContent.ContentId,
                Size = attachmentContent.Size
            });
        }

        [Route("{token:Guid}/translations")]
        [HttpGet]
        public Task<TranslationDto[]> TranslationsAsync(string token)
        {
            var questionnaireId = this.webTesterService.GetQuestionnaire(token)
                                  ?? throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound));

            var questionnaireView = this.questionnaireViewFactory.Load(new QuestionnaireViewInputModel(questionnaireId))
                                    ?? throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound));

            var actualTranslations = questionnaireView.Source.Translations.Select(x => x.Id).ToList();

            return Task.FromResult(this.translations
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