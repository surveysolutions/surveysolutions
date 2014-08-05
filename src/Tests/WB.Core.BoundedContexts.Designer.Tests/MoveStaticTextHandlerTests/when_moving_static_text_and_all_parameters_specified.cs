using System;
using Machine.Specifications;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests;

namespace WB.Core.BoundedContexts.Designer.Tests.MoveStaticTextHandlerTests
{
    internal class when_moving_static_text_and_all_parameters_specified : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(new StaticTextAdded() { EntityId = entityId, ParentId = chapterId });
            questionnaire.Apply(new NewGroupAdded { PublicKey = targetEntityId });

            eventContext = new EventContext();
        };

        Because of = () =>            
                questionnaire.MoveStaticText(entityId: entityId, responsibleId: responsibleId, targetEntityId: targetEntityId, targetIndex: targetIndex);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_QuestionnaireItemMoved_event = () =>
            eventContext.ShouldContainEvent<QuestionnaireItemMoved>();

        It should_raise_QuestionnaireItemMoved_event_with_PublicKey_specified = () =>
            eventContext.GetSingleEvent<QuestionnaireItemMoved>().PublicKey.ShouldEqual(entityId);

        It should_raise_QuestionnaireItemMoved_event_with_ResponsibleId_specified = () =>
            eventContext.GetSingleEvent<QuestionnaireItemMoved>().ResponsibleId.ShouldEqual(responsibleId);

        It should_raise_QuestionnaireItemMoved_event_with_GroupKey_specified = () =>
            eventContext.GetSingleEvent<QuestionnaireItemMoved>().GroupKey.ShouldEqual(targetEntityId);

        It should_raise_QuestionnaireItemMoved_event_with_TargetIndex_specified = () =>
            eventContext.GetSingleEvent<QuestionnaireItemMoved>().TargetIndex.ShouldEqual(targetIndex);
        
        private static EventContext eventContext;
        private static Questionnaire questionnaire;
        private static Guid entityId = Guid.Parse("11111111111111111111111111111111");
        private static Guid targetEntityId = Guid.Parse("22222222222222222222222222222222");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static int targetIndex = 0;
        
    }
}