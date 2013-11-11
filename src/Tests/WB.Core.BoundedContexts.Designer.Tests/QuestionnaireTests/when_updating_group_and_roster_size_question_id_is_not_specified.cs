using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests
{
    internal class when_updating_group_and_roster_size_question_id_is_not_specified : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            rosterSizeQuestionId = null;

            questionnaire = CreateQuestionnaireWithOneGroup(responsibleId, groupId: groupId);

            eventContext = new EventContext();
        };

        Because of = () =>
            questionnaire.UpdateGroup(groupId, responsibleId, "title", Propagate.None, rosterSizeQuestionId, null, null);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_GroupUpdated_event = () =>
            eventContext.ShouldContainEvent<GroupUpdated>();

        It should_raise_GroupUpdated_event_with_IsRoster_equal_false = () =>
            eventContext.GetSingleEvent<GroupUpdated>()
                .IsRoster.ShouldEqual(false);

        private static EventContext eventContext;
        private static Questionnaire questionnaire;
        private static Guid responsibleId;
        private static Guid groupId;
        private static Guid? rosterSizeQuestionId;
    }
}