using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.QuestionnaireCompilationForOldVersions;
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
        private readonly IExpressionProcessorGenerator expressionProcessorGenerator;

        private readonly IQuestionnaireViewFactory questionnaireViewFactory;
        private readonly IExpressionsPlayOrderProvider expressionsPlayOrderProvider;
        private readonly IDesignerEngineVersionService engineVersionService;
        private readonly IQuestionnaireVerifier questionnaireVerifier;
        private readonly IAttachmentService attachmentService;
        private readonly IPlainStorageAccessor<TranslationInstance> translations;
        private readonly IWebTesterService webTesterService;
        private readonly IQuestionnaireCompilationVersionService questionnaireCompilationVersionService;

        public WebTesterController(IExpressionProcessorGenerator expressionProcessorGenerator,
            IQuestionnaireViewFactory questionnaireViewFactory,
            IExpressionsPlayOrderProvider expressionsPlayOrderProvider,
            IDesignerEngineVersionService engineVersionService, 
            IAttachmentService attachmentService,
            IPlainStorageAccessor<TranslationInstance> translations,
            IWebTesterService webTesterService, 
            IQuestionnaireCompilationVersionService questionnaireCompilationVersionService, IQuestionnaireVerifier questionnaireVerifier)
        {
            this.expressionProcessorGenerator = expressionProcessorGenerator;
            this.questionnaireViewFactory = questionnaireViewFactory;
            this.expressionsPlayOrderProvider = expressionsPlayOrderProvider;
            this.engineVersionService = engineVersionService;
            this.attachmentService = attachmentService;
            this.translations = translations;
            this.webTesterService = webTesterService;
            this.questionnaireCompilationVersionService = questionnaireCompilationVersionService;
            this.questionnaireVerifier = questionnaireVerifier;
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
        public Task<Questionnaire> QuestionnaireAsync(string token)
        {
            var questionnaireId = this.webTesterService.GetQuestionnaire(token) 
                                  ?? throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound));
            
            var questionnaireView = this.questionnaireViewFactory.Load(new QuestionnaireViewInputModel(questionnaireId))
                                  ?? throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound));

    
            if (this.questionnaireVerifier.CheckForErrors(questionnaireView).Any())
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.PreconditionFailed));
            }

            var specifiedCompilationVersion = this.questionnaireCompilationVersionService.GetById(questionnaireId)?.Version;

            var versionToCompileAssembly = specifiedCompilationVersion ?? Math.Max(20,
                this.engineVersionService.GetQuestionnaireContentVersion(questionnaireView.Source));

            string resultAssembly;
            try
            {
                GenerationResult generationResult = this.expressionProcessorGenerator.GenerateProcessorStateAssembly(
                    questionnaireView.Source,
                    versionToCompileAssembly,
                    out resultAssembly);
                if (!generationResult.Success)
                    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.PreconditionFailed));
            }
            catch (Exception)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.PreconditionFailed));
            }

            var questionnaire = questionnaireView.Source.Clone();
            var readOnlyQuestionnaireDocument = questionnaireView.Source.AsReadOnly();
            questionnaire.ExpressionsPlayOrder = this.expressionsPlayOrderProvider.GetExpressionsPlayOrder(readOnlyQuestionnaireDocument);

            questionnaire.DependencyGraph = this.expressionsPlayOrderProvider
                .GetDependencyGraph(readOnlyQuestionnaireDocument).ToDictionary(x => x.Key, x => x.Value.ToArray());

            questionnaire.ValidationDependencyGraph = this.expressionsPlayOrderProvider
                .GetValidationDependencyGraph(readOnlyQuestionnaireDocument)
                .ToDictionary(x => x.Key, x => x.Value.ToArray());

            questionnaire.Macros = null;
            questionnaire.IsUsingExpressionStorage = versionToCompileAssembly > 19;

            return Task.FromResult(new Questionnaire
            {
                Document = questionnaire,
                Assembly = resultAssembly
            });
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
            
            return Task.FromResult(this.translations.Query(_ => 
                _.Where(x => x.QuestionnaireId == Guid.Parse(token) && actualTranslations.Contains(x.TranslationId))
                    .ToList()).Select(x => new TranslationDto
            {
                Value = x.Value,
                Type = x.Type,
                TranslationId = x.TranslationId,
                QuestionnaireEntityId = x.QuestionnaireEntityId,
                TranslationIndex = x.TranslationIndex
            }).ToArray());
        }
    }
}