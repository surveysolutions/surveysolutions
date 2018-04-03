using System;
using FluentAssertions;
using Main.Core.Documents;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.DenormalizerStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Tests.Abc;


namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireDenormalizerTests
{
    internal class when_ImportQuestionnaire : QuestionnaireDenormalizerTestsContext
    {
        [NUnit.Framework.Test]
        public void should_list_of_macroses_contains_macros_from_replaced_questionnaire_only()
        {
            Guid userId = Id.g1;
            Guid macrosWithBefore = Id.g2;

            var questionnaireDocument = CreateQuestionnaireDocument();
            questionnaireDocument.Macros.Add(macrosWithBefore, new Macro() { Description = "before" });

            var documentStorage = new InMemoryReadSideRepositoryAccessor<QuestionnaireDocument>();
            documentStorage.Store(questionnaireDocument, questionnaireDocument.PublicKey);
            var denormalizer = CreateQuestionnaireDenormalizer(questionnaire: questionnaireDocument, sharedPersons: macrosWithBefore.ToEnumerable());

            // Act
            denormalizer.ImportQuestionnaire(userId, questionnaireDocument);

            // Assert
            documentStorage.GetById(questionnaireDocument.PublicKey).Macros.Keys.Should().ContainSingle(x => x == macrosWithBefore);
        }
    }
}
