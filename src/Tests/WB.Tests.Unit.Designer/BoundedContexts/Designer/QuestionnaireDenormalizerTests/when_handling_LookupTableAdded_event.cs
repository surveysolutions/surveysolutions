using System;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto.LookupTables;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireDenormalizerTests
{
    internal class when_handling_LookupTableAdded_event : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            evnt = Create.Event.LookupTableAdded(questionnaireId, entityId);

            questionnaire = Create.QuestionnaireDocument(questionnaireId);

            denormalizer = CreateQuestionnaireDenormalizer(questionnaire: questionnaire);
        };

        Because of = () => denormalizer.AddLookupTable(evnt);

        It should_add_one_lookup_table = () =>
            questionnaire.LookupTables.Count.ShouldEqual(1);

        It should_add_lookup_table_with_key_equals_entity_id = () =>
           questionnaire.LookupTables.ContainsKey(entityId).ShouldBeTrue();

        It should_add_empty_lookup_table = () =>
           questionnaire.LookupTables[entityId].TableName.ShouldBeNull();

        private static Guid questionnaireId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid entityId = Guid.Parse("11111111111111111111111111111111");
        private static Questionnaire denormalizer;
        private static QuestionnaireDocument questionnaire;
        private static LookupTableAdded evnt;
    }
}