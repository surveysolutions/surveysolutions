using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Caching;
using System.Web.Http;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.QuestionnaireCompilationForOldVersions;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;

namespace WB.UI.Designer.Api.WebTester
{
    public class QuestionnairePackageComposer : IQuestionnairePackageComposer
    {
        private readonly IExpressionProcessorGenerator expressionProcessorGenerator;
        private readonly IPlainStorageAccessor<QuestionnaireChangeRecord> questionnaireChangeItemStorage;
        private readonly IQuestionnaireViewFactory questionnaireViewFactory;
        private readonly IExpressionsPlayOrderProvider expressionsPlayOrderProvider;
        private readonly IDesignerEngineVersionService engineVersionService;
        private readonly IQuestionnaireCompilationVersionService questionnaireCompilationVersionService;
        private readonly IQuestionnaireVerifier questionnaireVerifier;

        public QuestionnairePackageComposer(IExpressionProcessorGenerator expressionProcessorGenerator,
            IPlainStorageAccessor<QuestionnaireChangeRecord> questionnaireChangeItemStorage,
            IQuestionnaireViewFactory questionnaireViewFactory,
            IExpressionsPlayOrderProvider expressionsPlayOrderProvider,
            IDesignerEngineVersionService engineVersionService,
            IQuestionnaireCompilationVersionService questionnaireCompilationVersionService,
            IQuestionnaireVerifier questionnaireVerifier)
        {
            this.expressionProcessorGenerator = expressionProcessorGenerator;
            this.questionnaireChangeItemStorage = questionnaireChangeItemStorage;
            this.questionnaireViewFactory = questionnaireViewFactory;
            this.expressionsPlayOrderProvider = expressionsPlayOrderProvider;
            this.engineVersionService = engineVersionService;
            this.questionnaireCompilationVersionService = questionnaireCompilationVersionService;
            this.questionnaireVerifier = questionnaireVerifier;
        }

        static readonly MemoryCache Cache = new MemoryCache("CompilationPackages");

        public Questionnaire ComposeQuestionnaire(Guid questionnaireId)
        {
            var maxSequenceByQuestionnaire = this.questionnaireChangeItemStorage
                .Query(x => x.Where(y => y.QuestionnaireId == questionnaireId.FormatGuid()).Select(y => (int?)y.Sequence).Max());
            
            var cacheKey = $"{questionnaireId}.{maxSequenceByQuestionnaire}";

            var cacheEntry = Cache.Get(cacheKey) as Lazy<Questionnaire>;

            if (cacheEntry == null)
            {
                cacheEntry = new Lazy<Questionnaire>(() => ComposeQuestionnaireImpl(questionnaireId));
                Cache.Add(cacheKey, cacheEntry, new CacheItemPolicy
                {
                    SlidingExpiration = TimeSpan.FromMinutes(10)
                });
            }

            try
            {
                return cacheEntry.Value;
            }
            catch
            {
                Cache.Remove(cacheKey);
                throw;
            }
        }

        private Questionnaire ComposeQuestionnaireImpl(Guid questionnaireId)
        {
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

            questionnaire.DependencyGraph = this.expressionsPlayOrderProvider.GetDependencyGraph(readOnlyQuestionnaireDocument);

            questionnaire.ValidationDependencyGraph = this.expressionsPlayOrderProvider
                .GetValidationDependencyGraph(readOnlyQuestionnaireDocument)
                .ToDictionary(x => x.Key, x => x.Value.ToArray());

            questionnaire.Macros = null;
            questionnaire.IsUsingExpressionStorage = versionToCompileAssembly > 19;

            return new Questionnaire
            {
                Document = questionnaire,
                Assembly = resultAssembly
            };
        }
    }
}
