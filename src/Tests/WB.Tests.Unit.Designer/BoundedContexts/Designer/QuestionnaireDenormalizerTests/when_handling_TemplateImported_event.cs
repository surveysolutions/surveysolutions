using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.DenormalizerStorage;
using WB.Core.BoundedContexts.Designer.Aggregates;

using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions.Documents;



namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireDenormalizerTests
{
    internal class when_ImportQuestionnaire : QuestionnaireDenormalizerTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaireDocument = CreateQuestionnaireDocument();
            questionnaireDocument.Macros.Add(macrosWithBefore, new Macro() { Description = "before" });

            documentStorage = new InMemoryReadSideRepositoryAccessor<QuestionnaireDocument>();
            documentStorage.Store(questionnaireDocument, questionnaireDocument.PublicKey);
            denormalizer = CreateQuestionnaireDenormalizer(questionnaire: questionnaireDocument, sharedPersons: macrosWithBefore.ToEnumerable());
            BecauseOf();
        }

        private void BecauseOf() =>
            denormalizer.ImportQuestionnaire(userId, questionnaireDocument);

        [NUnit.Framework.Test] public void should_list_of_macroses_contains_macros_from_replaced_questionnaire_only () =>
           documentStorage.GetById(questionnaireDocument.PublicKey).Macros.Keys.ShouldContainOnly(macrosWithBefore);

        private static Questionnaire denormalizer;
        private static QuestionnaireDocument questionnaireDocument;
        
        private static Guid userId = Guid.Parse("1111111111111111111111111111111A");
        private static Guid macrosWithBefore = Guid.Parse("11111111111111111111111111111111");
        private static InMemoryReadSideRepositoryAccessor<QuestionnaireDocument> documentStorage;
    }
}
