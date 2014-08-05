using System;
using Machine.Specifications;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests;

namespace WB.Core.BoundedContexts.Designer.Tests.UpdateStaticTextHandlerTests
{
    internal class when_updating_static_text_and_all_parameters_specified : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(new StaticTextAdded() { EntityId = entityId, ParentId = chapterId });

            eventContext = new EventContext();
        };

        Because of = () =>            
                questionnaire.UpdateStaticText(entityId: entityId, text: text, responsibleId: responsibleId);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_StaticTextUpdated_event = () =>
            eventContext.ShouldContainEvent<StaticTextUpdated>();

        It should_raise_StaticTextUpdated_event_with_EntityId_specified = () =>
            eventContext.GetSingleEvent<StaticTextUpdated>().EntityId.ShouldEqual(entityId);

        It should_raise_StaticTextUpdated_event_with_Text_specified = () =>
            eventContext.GetSingleEvent<StaticTextUpdated>().Text.ShouldEqual(text);

        private static EventContext eventContext;
        private static Questionnaire questionnaire;
        private static Guid entityId = Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static string text = "some text";
    }
}