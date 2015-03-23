using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests
{
    internal class when_adding_roster_group_by_question_and_roster_size_question_id_is_not_specified : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            rosterSizeQuestionId = null;

            questionnaire = CreateQuestionnaire(responsibleId);

            eventContext = new EventContext();
        };

        Because of = () =>
            questionnaire.AddGroupAndMoveIfNeeded(groupId, responsibleId, "title",null, rosterSizeQuestionId, null, null, null, false,
                RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_GroupStoppedBeingARoster_event = () =>
            eventContext.ShouldContainEvent<GroupStoppedBeingARoster>();

        It should_raise_GroupStoppedBeingARoster_event_with_GroupId_specified = () =>
            eventContext.GetSingleEvent<GroupStoppedBeingARoster>()
                .GroupId.ShouldEqual(groupId);

        private static EventContext eventContext;
        private static Questionnaire questionnaire;
        private static Guid responsibleId;
        private static Guid? rosterSizeQuestionId;
        private static Guid groupId;
    }
}