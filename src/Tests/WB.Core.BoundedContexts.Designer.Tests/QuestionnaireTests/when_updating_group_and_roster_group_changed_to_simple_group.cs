using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using NUnit.Framework.Constraints;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests
{
    internal class when_updating_group_and_roster_group_changed_to_simple_group : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            questionnaire = CreateQuestionnaireWithOneGroup(responsibleId, groupId: groupId);

            eventContext = new EventContext();
        };

        private Because of = () =>
            questionnaire.UpdateGroup(groupId, responsibleId, "title", null, null, null, isRoster: false,
                rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null);

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
        private static Guid groupId;
    }
}