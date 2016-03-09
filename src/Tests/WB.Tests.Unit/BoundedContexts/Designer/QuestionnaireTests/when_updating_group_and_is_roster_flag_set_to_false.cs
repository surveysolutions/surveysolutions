﻿using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests
{
    internal class when_updating_group_and_is_roster_flag_set_to_false : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            questionnaire = CreateQuestionnaireWithOneGroup(responsibleId, groupId: groupId);

            eventContext = new EventContext();
        };

        Because of = () =>
            questionnaire.UpdateGroup(groupId, responsibleId, "title",null, null, null, null, hideIfDisabled: false, isRoster: false,
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