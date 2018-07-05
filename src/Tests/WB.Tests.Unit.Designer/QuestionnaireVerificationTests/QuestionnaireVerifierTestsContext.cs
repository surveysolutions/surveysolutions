using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.TopologicalSorter;
using WB.Core.SharedKernels.Questionnaire.Translations;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    [TestOf(typeof(QuestionnaireVerifier))]
    internal class QuestionnaireVerifierTestsContext
    {
        protected static QuestionnaireVerifier CreateQuestionnaireVerifier(
            IExpressionProcessor expressionProcessor = null,
            ISubstitutionService substitutionService = null, 
            IKeywordsProvider keywordsProvider = null, 
            IExpressionProcessorGenerator expressionProcessorGenerator = null,
            IMacrosSubstitutionService macrosSubstitutionService = null,
            ILookupTableService lookupTableService = null,
            IAttachmentService attachmentService = null,
            ITopologicalSorter<Guid> topologicalSorter = null,
            IQuestionnaireTranslator questionnaireTranslator = null)
            => Create.QuestionnaireVerifier(
                expressionProcessor: expressionProcessor,
                substitutionService: substitutionService,
                keywordsProvider: keywordsProvider,
                expressionProcessorGenerator: expressionProcessorGenerator,
                macrosSubstitutionService: macrosSubstitutionService,
                lookupTableService: lookupTableService,
                attachmentService: attachmentService,
                topologicalSorter: topologicalSorter,
                questionnaireTranslator: questionnaireTranslator);

        protected static QuestionnaireDocument CreateQuestionnaireDocument(params IComposite[] questionnaireChildren)
        {
            return Create.QuestionnaireDocument(children: questionnaireChildren);
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocumentWithOneChapter(params IComposite[] chapterChildren)
        {
            return Create.QuestionnaireDocumentWithOneChapter(chapterChildren);
        }

        public static IExpressionProcessorGenerator CreateExpressionProcessorGenerator(ICodeGenerator codeGenerator = null, IDynamicCompiler dynamicCompiler = null)
        {
            return new QuestionnaireExpressionProcessorGenerator(
                    new RoslynCompiler(),
                    Create.CodeGenerator(),
                    Create.CodeGeneratorV2(),
                    Create.DynamicCompilerSettingsProvider());
        }

        public static QuestionnaireVerificationMessage FindWarningForEntityWithId(IEnumerable<QuestionnaireVerificationMessage> errors, string code, Guid entityId)
        {
            return errors.FirstOrDefault(message
                => message.MessageLevel == VerificationMessageLevel.Warning
                   && message.Code == code &&
                   message.References.First().Id == entityId);
        }
    }
}
