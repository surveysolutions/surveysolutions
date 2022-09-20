using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Designer.CodeGenerationV2;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;
using WB.Core.BoundedContexts.Designer.QuestionnaireCompilationForOldVersions;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.TopologicalSorter;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.BoundedContexts.Designer.Verifier
{
    internal class QuestionnaireVerifier : IQuestionnaireVerifier
    {
        private readonly IEnumerable<IPartialVerifier> verifiers;

        private readonly IExpressionProcessorGenerator expressionProcessorGenerator;
        private readonly IDesignerEngineVersionService engineVersionService;
        private readonly ITranslationsService translationService;
        private readonly IQuestionnaireTranslator questionnaireTranslator;
        private readonly IQuestionnaireCompilationVersionService questionnaireCompilationVersionService;
        private readonly ITopologicalSorter<Guid> topologicalSorter;
        private readonly IExpressionsPlayOrderProvider graphProvider;
        private readonly IQuestionnaireCodeGenerationPackageFactory generationPackageFactory;


        public QuestionnaireVerifier(IExpressionProcessor expressionProcessor, 
            IFileSystemAccessor fileSystemAccessor, 
            ISubstitutionService substitutionService, 
            IKeywordsProvider keywordsProvider, 
            IExpressionProcessorGenerator expressionProcessorGenerator, 
            IDesignerEngineVersionService engineVersionService, 
            IMacrosSubstitutionService macrosSubstitutionService, 
            ILookupTableService lookupTableService, 
            IAttachmentService attachmentService, 
            ITopologicalSorter<Guid> topologicalSorter, 
            ITranslationsService translationService, 
            IQuestionnaireTranslator questionnaireTranslator, 
            IQuestionnaireCompilationVersionService questionnaireCompilationVersionService, 
            IDynamicCompilerSettingsProvider compilerSettings,
            IExpressionsPlayOrderProvider graphProvider,
            ICategoriesService categoriesService,
            IQuestionnaireCodeGenerationPackageFactory generationPackageFactory)
        {
            this.expressionProcessorGenerator = expressionProcessorGenerator;
            this.engineVersionService = engineVersionService;
            this.translationService = translationService;
            this.questionnaireTranslator = questionnaireTranslator;
            this.questionnaireCompilationVersionService = questionnaireCompilationVersionService;
            this.topologicalSorter = topologicalSorter;
            this.graphProvider = graphProvider;
            this.generationPackageFactory = generationPackageFactory;

            verifiers = new IPartialVerifier[]
            {
                new QuestionVerifications(substitutionService, categoriesService),
                new GroupVerifications(fileSystemAccessor),
                new AttachmentVerifications(attachmentService, keywordsProvider), 
                new ExpressionVerifications(macrosSubstitutionService, expressionProcessor, compilerSettings), 
                new LookupVerifications(lookupTableService, keywordsProvider), 
                new MacroVerifications(), 
                new QuestionnaireVerifications(substitutionService, keywordsProvider), 
                new StaticTextVerifications(), 
                new TranslationVerifications(translationService), 
                new VariableVerifications(substitutionService),
                new CategoriesVerifications(keywordsProvider, categoriesService), 
            };
        }

       
        public IEnumerable<QuestionnaireVerificationMessage> CompileAndVerify(QuestionnaireView questionnaireView,
            int? targetCompilationVersion, out string resultAssembly)
        {
            resultAssembly = string.Empty;
            var questionnaire = questionnaireView.Source;

            var readOnlyQuestionnaireDocument = new ReadOnlyQuestionnaireDocument(questionnaire);
            var verificationMessagesByQuestionnaire = PreVerify(readOnlyQuestionnaireDocument);
            if(verificationMessagesByQuestionnaire.Any())
                return verificationMessagesByQuestionnaire;

            var readOnlyQuestionnaireDocumentWithCache = new ReadOnlyQuestionnaireDocumentWithCache(questionnaire);
            
            List<ReadOnlyQuestionnaireDocumentWithCache> translatedQuestionnaires = new List<ReadOnlyQuestionnaireDocumentWithCache>();
            questionnaire.Translations.ForEach(t =>
            {
                var translation = this.translationService.Get(questionnaire.PublicKey, t.Id);
                var translatedQuestionnaireDocument = this.questionnaireTranslator.Translate(questionnaire, translation);
                translatedQuestionnaires.Add(new ReadOnlyQuestionnaireDocumentWithCache(translatedQuestionnaireDocument, t.Name));
            });

            var multiLanguageQuestionnaireDocument = new MultiLanguageQuestionnaireDocument(
                readOnlyQuestionnaireDocumentWithCache,
                translatedQuestionnaires.ToArray(),
                questionnaireView.SharedPersons);

            foreach (var verifier in this.verifiers)
            {
                IEnumerable<QuestionnaireVerificationMessage> errors = verifier.Verify(multiLanguageQuestionnaireDocument);
                verificationMessagesByQuestionnaire.AddRange(errors);
            }

            var errorsByCircularReferences = 
                this.ErrorsByCircularReferences(readOnlyQuestionnaireDocumentWithCache);
            
            verificationMessagesByQuestionnaire.AddRange(errorsByCircularReferences);

            if (verificationMessagesByQuestionnaire.Any(e => e.MessageLevel == VerificationMessageLevel.Critical))
                return verificationMessagesByQuestionnaire;

            List<QuestionnaireVerificationMessage> verificationMessagesByCompiler = new List<QuestionnaireVerificationMessage>();

            var questionnaireVersionToCompileAssembly = 
                targetCompilationVersion 
                ?? ( this.questionnaireCompilationVersionService.GetById(questionnaire.PublicKey)?.Version
                     ?? Math.Max(20, this.engineVersionService.GetQuestionnaireContentVersion(questionnaire)));

            var compiledReadyDocument = questionnaireView.GetCompiledReadyDocument();
            var package = generationPackageFactory.Generate(compiledReadyDocument, questionnaire.PublicKey);

            var compilationResult = this.expressionProcessorGenerator.GenerateProcessorStateAssembly(
                package, questionnaireVersionToCompileAssembly, out resultAssembly);

            if (!compilationResult.Success)
            {
                var elementsWithErrorMessages =
                    compilationResult.Diagnostics.GroupBy(x => x.Location, x => x.Message);

                foreach (var elementWithErrors in elementsWithErrorMessages)
                {
                    verificationMessagesByCompiler.Add(CreateExpressionSyntaxError(
                        new ExpressionLocation(elementWithErrors.Key), elementWithErrors.ToList()));
                }
            }

            return verificationMessagesByQuestionnaire.Concat(verificationMessagesByCompiler);
        }

        private List<QuestionnaireVerificationMessage> PreVerify(ReadOnlyQuestionnaireDocument readOnlyQuestionnaireDocument)
        {
            var preCheckVerifications = new QuestionnairePreVerifications();
            var verificationMessagesByQuestionnaire = new List<QuestionnaireVerificationMessage>();
            verificationMessagesByQuestionnaire.AddRange(preCheckVerifications.Verify(readOnlyQuestionnaireDocument));
            
            return verificationMessagesByQuestionnaire;
        }

        public IEnumerable<QuestionnaireVerificationMessage> GetAllErrors(QuestionnaireView questionnaireView, 
            bool includeWarnings = false)
        {
            if (includeWarnings)
                return this.CompileAndVerify(questionnaireView, null, out string assembly);
            else
                return this.CompileAndVerify(questionnaireView, null, out string assembly)
                    .Where(x => x.MessageLevel != VerificationMessageLevel.Warning);
        }

        private IEnumerable<QuestionnaireVerificationMessage> ErrorsByCircularReferences(ReadOnlyQuestionnaireDocumentWithCache questionnaire)
        {
            var dependencyGraph = graphProvider.GetDependencyGraph(questionnaire);
            var cycles = topologicalSorter.DetectCycles(dependencyGraph);

            foreach (var cycle in cycles)
            {
                var references =
                    cycle.Select(guid => questionnaire.Find<IComposite>(guid))
                        .Where(x=> x != null)
                        .Select(x => QuestionnaireEntityReference.CreateFrom(x))
                        .ToArray();

                yield return QuestionnaireVerificationMessage.Error("WB0056", VerificationMessages.WB0056_EntityShouldNotHaveCircularReferences, references);
            }
        }

        private static QuestionnaireVerificationMessage CreateExpressionSyntaxError(ExpressionLocation expressionLocation, IEnumerable<string> compilationErrorMessages)
        {
            QuestionnaireEntityReference reference;

            switch (expressionLocation.ItemType)
            {
                case ExpressionLocationItemType.Group:
                    reference = QuestionnaireEntityReference.CreateForGroup(expressionLocation.Id);
                    break;
                case ExpressionLocationItemType.Roster:
                    reference = QuestionnaireEntityReference.CreateForRoster(expressionLocation.Id);
                    break;
                case ExpressionLocationItemType.Question:
                    reference = QuestionnaireEntityReference.CreateForQuestion(expressionLocation.Id);
                    break;
                case ExpressionLocationItemType.StaticText:
                    reference = QuestionnaireEntityReference.CreateForStaticText(expressionLocation.Id);
                    break;
                case ExpressionLocationItemType.LookupTable:
                    reference = QuestionnaireEntityReference.CreateForLookupTable(expressionLocation.Id);
                    break;
                case ExpressionLocationItemType.Variable:
                    reference = QuestionnaireEntityReference.CreateForVariable(expressionLocation.Id);
                    break;
                case ExpressionLocationItemType.Questionnaire:
                    return QuestionnaireVerificationMessage.Error("WB0096", VerificationMessages.WB0096_GeneralCompilationError);
                default:
                    throw new ArgumentException("expressionLocation");
            }

            switch (expressionLocation.ExpressionType)
            {
                case ExpressionLocationType.Validation:
                    reference.IndexOfEntityInProperty = expressionLocation.ExpressionPosition;
                    return QuestionnaireVerificationMessage.Error("WB0002",
                        VerificationMessages.WB0002_CustomValidationExpressionHasIncorrectSyntax, compilationErrorMessages, reference);
                case ExpressionLocationType.Condition:
                    return QuestionnaireVerificationMessage.Error("WB0003",
                        VerificationMessages.WB0003_CustomEnablementConditionHasIncorrectSyntax, compilationErrorMessages, reference);
                case ExpressionLocationType.Filter:
                    return QuestionnaireVerificationMessage.Error("WB0110",
                        VerificationMessages.WB0110_LinkedQuestionFilterExpresssionHasIncorrectSyntax, compilationErrorMessages, reference);
                case ExpressionLocationType.Expression:
                    return QuestionnaireVerificationMessage.Error("WB0027",
                        VerificationMessages.WB0027_VariableExpresssionHasIncorrectSyntax, compilationErrorMessages, reference);
                case ExpressionLocationType.CategoricalFilter:
                    return QuestionnaireVerificationMessage.Error("WB0062",
                        VerificationMessages.WB0062_OptionFilterExpresssionHasIncorrectSyntax, compilationErrorMessages, reference);
            }

            return QuestionnaireVerificationMessage.Error("WB0096", VerificationMessages.WB0096_GeneralCompilationError);
        }
    }
}
