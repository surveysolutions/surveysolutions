using System;
using Machine.Specifications;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.DeleteStaticTextHandlerTests
{
    internal class when_deleting_static_text_and_all_parameters_specified : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.AddStaticText(Create.Event.StaticTextAdded(entityId : entityId, parentId : chapterId ));

            eventContext = new EventContext();
        };

        Because of = () =>            
                questionnaire.DeleteStaticText(entityId: entityId, responsibleId: responsibleId);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_StaticTextDeleted_event = () =>
            eventContext.ShouldContainEvent<StaticTextDeleted>();

        It should_raise_StaticTextDeleted_event_with_EntityId_specified = () =>
            eventContext.GetSingleEvent<StaticTextDeleted>().EntityId.ShouldEqual(entityId);
        
        private static EventContext eventContext;
        private static Questionnaire questionnaire;
        private static Guid entityId = Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
    }
}