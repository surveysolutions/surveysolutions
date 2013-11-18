using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests
{
    internal class when_cloning_group_and_roster_size_question_id_is_not_specified : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            sourceGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            targetGroupId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            rosterSizeQuestionId = null;

            questionnaire = CreateQuestionnaireWithOneGroup(responsibleId, groupId: sourceGroupId);

            eventContext = new EventContext();
        };

        Because of = () =>
            questionnaire.CloneGroupWithoutChildren(targetGroupId, responsibleId, "title", rosterSizeQuestionId, null, null, null, sourceGroupId, 0);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_GroupStoppedBeingARoster_event = () =>
            eventContext.ShouldContainEvent<GroupStoppedBeingARoster>();

        It should_raise_GroupStoppedBeingARoster_event_with_GroupId_equal_to_target_group_id = () =>
            eventContext.GetSingleEvent<GroupStoppedBeingARoster>()
                .GroupId.ShouldEqual(targetGroupId);

        private static EventContext eventContext;
        private static Questionnaire questionnaire;
        private static Guid responsibleId;
        private static Guid sourceGroupId;
        private static Guid? rosterSizeQuestionId;
        private static Guid targetGroupId;
    }
}