using System;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire.Macros;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireDenormalizerTests
{
    internal class when_handling_duplicated_MacroAdded_event : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            evnt = Create.Event.MacroAdded(questionnaireId, entityId);

            questionnaire = Create.QuestionnaireDocument(questionnaireId);

            denormalizer = CreateQuestionnaireDenormalizer(questionnaire: questionnaire);
            denormalizer.AddMacro(evnt.Payload);
        };

        Because of = () => denormalizer.AddMacro(evnt.Payload);

        It should_not_add_extra_macro = () =>
            questionnaire.Macros.Count.ShouldEqual(1);

        It should_add_macro_with_key_equals_entity_id = () =>
            questionnaire.Macros.ContainsKey(entityId).ShouldBeTrue();

        It should_add_macro_with_empty_name = () =>
            questionnaire.Macros[entityId].Name.ShouldBeNull();

        It should_add_macro_with_empty_content = () =>
            questionnaire.Macros[entityId].Content.ShouldBeNull();

        It should_add_macro_with_empty_description = () =>
            questionnaire.Macros[entityId].Description.ShouldBeNull();

        private static Guid questionnaireId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid entityId = Guid.Parse("11111111111111111111111111111111");
        private static Questionnaire denormalizer;
        private static QuestionnaireDocument questionnaire;
        private static IPublishedEvent<MacroAdded> evnt;
    }
}