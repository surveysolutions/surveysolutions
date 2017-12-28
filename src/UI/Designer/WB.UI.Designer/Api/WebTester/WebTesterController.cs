using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Questionnaire.Api;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;

namespace WB.UI.Designer.Api.WebTester
{
    [RoutePrefix("api/webtester")]
    public class WebTesterController : ApiController
    {
        private readonly IExpressionProcessorGenerator expressionProcessorGenerator;

        private readonly IQuestionnaireViewFactory questionnaireViewFactory;
        private readonly IExpressionsPlayOrderProvider expressionsPlayOrderProvider;
        private readonly IDesignerEngineVersionService engineVersionService;
        private readonly IAttachmentService attachmentService;
        private readonly IPlainStorageAccessor<TranslationInstance> translations;


        public WebTesterController(IExpressionProcessorGenerator expressionProcessorGenerator,
            IQuestionnaireViewFactory questionnaireViewFactory,
            IExpressionsPlayOrderProvider expressionsPlayOrderProvider,
            IDesignerEngineVersionService engineVersionService, IAttachmentService attachmentService,
            IPlainStorageAccessor<TranslationInstance> translations)
        {
            this.expressionProcessorGenerator = expressionProcessorGenerator;
            this.questionnaireViewFactory = questionnaireViewFactory;
            this.expressionsPlayOrderProvider = expressionsPlayOrderProvider;
            this.engineVersionService = engineVersionService;
            this.attachmentService = attachmentService;
            this.translations = translations;
        }

        [Route("{token:Guid}/info", Name = "info")]
        [HttpGet]
        public Task<QuestionnaireLiteInfo> InfoAsync(string token)
        {
            var questionnaireView =
                this.questionnaireViewFactory.Load(new QuestionnaireViewInputModel(Guid.Parse(token)));

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
            //if (version < ApiVersion.CurrentTesterProtocolVersion)
            //    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.UpgradeRequired));

            var questionnaireView =
                this.questionnaireViewFactory.Load(new QuestionnaireViewInputModel(Guid.Parse(token)));
            //if (questionnaireView == null)
            //{
            //    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound));
            //}

            //if (!this.ValidateAccessPermissions(questionnaireView))
            //{
            //    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
            //}

            //if (this.questionnaireVerifier.CheckForErrors(questionnaireView).Any())
            //{
            //    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.PreconditionFailed));
            //}

            //var specifiedCompilationVersion = this.questionnaireCompilationVersionService.GetById(id)?.Version;

            var versionToCompileAssembly = Math.Max(20,
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
            questionnaire.ExpressionsPlayOrder =
                this.expressionsPlayOrderProvider.GetExpressionsPlayOrder(readOnlyQuestionnaireDocument);
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
            return Task.FromResult(this.translations.Query(_ => _.Where(x => x.QuestionnaireId == Guid.Parse(token)).ToList()).Select(x => new TranslationDto()
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