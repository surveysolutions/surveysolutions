using System;
using Machine.Specifications;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests;

namespace WB.Core.BoundedContexts.Designer.Tests.AddStaticTextHandlerTests
{
    internal class when_adding_static_text_to_chapter : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });

            eventContext = new EventContext();
        };

        Because of = () =>            
                questionnaire.AddStaticText(entityId: entityId, parentId: chapterId, text: text, responsibleId: responsibleId);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_StaticTextAdded_event = () =>
            eventContext.ShouldContainEvent<StaticTextAdded>();

        It should_raise_StaticTextAdded_event_with_EntityId_specified = () =>
            eventContext.GetSingleEvent<StaticTextAdded>().EntityId.ShouldEqual(entityId);

        It should_raise_StaticTextAdded_event_with_ParentId_specified = () =>
            eventContext.GetSingleEvent<StaticTextAdded>().ParentId.ShouldEqual(chapterId);

        It should_raise_StaticTextAdded_event_with_Text_specified = () =>
            eventContext.GetSingleEvent<StaticTextAdded>().Text.ShouldEqual(text);

        private static EventContext eventContext;
        private static Questionnaire questionnaire;
        private static Guid entityId = Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static string text = "some text";
    }
}