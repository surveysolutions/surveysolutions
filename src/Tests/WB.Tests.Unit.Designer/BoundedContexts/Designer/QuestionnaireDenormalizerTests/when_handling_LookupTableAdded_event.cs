using System;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.LookupTables;


namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireDenormalizerTests
{
    internal class when_handling_LookupTableAdded_event : QuestionnaireDenormalizerTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = Create.QuestionnaireDocument(questionnaireId, userId: creatorId);

            denormalizer = CreateQuestionnaireDenormalizer(questionnaire: questionnaire);
            BecauseOf();
        }

        private void BecauseOf() => denormalizer.AddLookupTable(new AddLookupTable(questionnaire.PublicKey, null, "", entityId, creatorId));

        [NUnit.Framework.Test] public void should_add_one_lookup_table () =>
            questionnaire.LookupTables.Count.ShouldEqual(1);

        [NUnit.Framework.Test] public void should_add_lookup_table_with_key_equals_entity_id () =>
           questionnaire.LookupTables.ContainsKey(entityId).ShouldBeTrue();

        [NUnit.Framework.Test] public void should_add_empty_lookup_table () =>
           questionnaire.LookupTables[entityId].TableName.ShouldBeNull();

        private static Guid questionnaireId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid entityId = Guid.Parse("11111111111111111111111111111111");
        private static Guid creatorId = Guid.Parse("A1111111111111111111111111111111");
        private static Questionnaire denormalizer;
        private static QuestionnaireDocument questionnaire;
        
    }
}