using System;
using Machine.Specifications;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests;

namespace WB.Core.BoundedContexts.Designer.Tests.CloneStaticTextHandlerTests
{
    internal class when_cloning_static_text_and_all_parameters_specified : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(new StaticTextAdded() { EntityId = sourceEntityId, ParentId = chapterId });

            eventContext = new EventContext();
        };

        Because of = () =>
            questionnaire.CloneStaticText(entityId: targetEntityId, responsibleId: responsibleId, sourceEntityId: sourceEntityId);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_StaticTextCloned_event = () =>
            eventContext.ShouldContainEvent<StaticTextCloned>();

        It should_raise_StaticTextCloned_event_with_EntityId_specified = () =>
            eventContext.GetSingleEvent<StaticTextCloned>().EntityId.ShouldEqual(targetEntityId);

        It should_raise_StaticTextCloned_event_with_SourceEntityId_specified = () =>
                    eventContext.GetSingleEvent<StaticTextCloned>().SourceEntityId.ShouldEqual(sourceEntityId);

        private static EventContext eventContext;
        private static Questionnaire questionnaire;
        private static Guid sourceEntityId = Guid.Parse("11111111111111111111111111111111");
        private static Guid targetEntityId = Guid.Parse("22222222222222222222222222222222");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
    }
}