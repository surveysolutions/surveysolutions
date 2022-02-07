using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.QuestionnaireCompilationForOldVersions;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.UI.Designer.Api.WebTester;

namespace WB.UI.Designer.Controllers.Api.WebTester
{
    public class QuestionnairePackageComposer : IQuestionnairePackageComposer
    {
        private readonly IExpressionProcessorGenerator expressionProcessorGenerator;
        private readonly DesignerDbContext questionnaireChangeItemStorage;
        private readonly IQuestionnaireViewFactory questionnaireViewFactory;
        private readonly IExpressionsPlayOrderProvider expressionsPlayOrderProvider;
        private readonly IDesignerEngineVersionService engineVersionService;
        private readonly IQuestionnaireCompilationVersionService questionnaireCompilationVersionService;
        private readonly IQuestionnaireVerifier questionnaireVerifier;
        private readonly IQuestionnaireCacheStorage questionnaireCacheStorage;

        public QuestionnairePackageComposer(IExpressionProcessorGenerator expressionProcessorGenerator,
            DesignerDbContext questionnaireChangeItemStorage,
            IQuestionnaireViewFactory questionnaireViewFactory,
            IExpressionsPlayOrderProvider expressionsPlayOrderProvider,
            IDesignerEngineVersionService engineVersionService,
            IQuestionnaireCompilationVersionService questionnaireCompilationVersionService,
            IQuestionnaireVerifier questionnaireVerifier,
            IQuestionnaireCacheStorage questionnaireCacheStorage)
        {
            this.expressionProcessorGenerator = expressionProcessorGenerator;
            this.questionnaireChangeItemStorage = questionnaireChangeItemStorage;
            this.questionnaireViewFactory = questionnaireViewFactory;
            this.expressionsPlayOrderProvider = expressionsPlayOrderProvider;
            this.engineVersionService = engineVersionService;
            this.questionnaireCompilationVersionService = questionnaireCompilationVersionService;
            this.questionnaireVerifier = questionnaireVerifier;
            this.questionnaireCacheStorage = questionnaireCacheStorage;
        }


        public Questionnaire ComposeQuestionnaire(Guid questionnaireId)
        {
            var maxSequenceByQuestionnaire = this.questionnaireChangeItemStorage.QuestionnaireChangeRecords
                .Where(y => y.QuestionnaireId == questionnaireId.FormatGuid())
                .Select(y => (int?)y.Sequence)
                .Max();

            var questionnaireKey = $"{questionnaireId}.{maxSequenceByQuestionnaire}";

            try
            {
                return questionnaireCacheStorage.GetOrCreate(questionnaireKey, questionnaireId, ComposeQuestionnaireImpl);
            }
            catch
            {
                questionnaireCacheStorage.Remove(questionnaireKey);
                throw;
            }
        }

        private Questionnaire ComposeQuestionnaireImpl(Guid questionnaireId)
        {
            var questionnaireView = this.questionnaireViewFactory.Load(new QuestionnaireViewInputModel(questionnaireId));
            if (questionnaireView == null)
                throw new InvalidOperationException("Questionnaire not found.");

            var specifiedCompilationVersion = this.questionnaireCompilationVersionService.GetById(questionnaireId)?.Version;

            var versionToCompileAssembly = specifiedCompilationVersion ?? Math.Max(20,
                                               this.engineVersionService.GetQuestionnaireContentVersion(questionnaireView.Source));

            string resultAssembly;
            List<QuestionnaireVerificationMessage> verificationResult;
            try
            {
                verificationResult = this.questionnaireVerifier.CompileAndVerify(questionnaireView,
                    versionToCompileAssembly,
                    out resultAssembly).ToList();
            }
            catch (Exception)
            {
                throw new ComposeException();
            }
            
            if (verificationResult.Any(x => x.MessageLevel != VerificationMessageLevel.Warning))
                throw new ComposeException();

            var questionnaire = questionnaireView.Source.Clone();
            var readOnlyQuestionnaireDocument = new ReadOnlyQuestionnaireDocumentWithCache(questionnaireView.Source);
            questionnaire.ExpressionsPlayOrder = this.expressionsPlayOrderProvider.GetExpressionsPlayOrder(readOnlyQuestionnaireDocument);

            questionnaire.DependencyGraph = this.expressionsPlayOrderProvider.GetDependencyGraph(readOnlyQuestionnaireDocument);

            questionnaire.ValidationDependencyGraph = this.expressionsPlayOrderProvider
                .GetValidationDependencyGraph(readOnlyQuestionnaireDocument)
                .ToDictionary(x => x.Key, x => x.Value.ToArray());

            questionnaire.Macros = new Dictionary<Guid, Macro>();
            questionnaire.IsUsingExpressionStorage = versionToCompileAssembly > 19;

            return new Questionnaire(document: questionnaire, assembly: resultAssembly);
        }
    }

    public class ComposeException : Exception
    {
    }
}
