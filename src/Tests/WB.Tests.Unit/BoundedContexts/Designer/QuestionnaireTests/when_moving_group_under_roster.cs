﻿using System;
using Machine.Specifications;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests
{
    internal class when_moving_group_under_roster : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(new NewGroupAdded { PublicKey = groupId, GroupText = "group to move", ParentGroupPublicKey = chapterId });
            questionnaire.Apply(new NewGroupAdded { PublicKey = parentRosterId, ParentGroupPublicKey = chapterId });
            questionnaire.Apply(new GroupBecameARoster(responsibleId, parentRosterId));

            eventContext = new EventContext();
        };

        Because of = () => questionnaire.MoveGroup(groupId, parentRosterId, targetIndex, responsibleId);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_QuestionnaireItemMoved_event = () =>
            eventContext.ShouldContainEvent<QuestionnaireItemMoved>();

        It should_raise_QuestionnaireItemMoved_event_with_GroupId_specified = () =>
            eventContext.GetSingleEvent<QuestionnaireItemMoved>()
                .PublicKey.ShouldEqual(groupId);

        It should_raise_QuestionnaireItemMoved_event_with_ParentGroupId_specified = () =>
            eventContext.GetSingleEvent<QuestionnaireItemMoved>()
                .GroupKey.ShouldEqual(parentRosterId);

        It should_raise_QuestionnaireItemMoved_event_with_TargetIndex_specified = () =>
            eventContext.GetSingleEvent<QuestionnaireItemMoved>()
                .TargetIndex.ShouldEqual(targetIndex);

        private static Questionnaire questionnaire;
        private static Guid groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static Guid parentRosterId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static int targetIndex = 0;
        private static EventContext eventContext;
    }
}