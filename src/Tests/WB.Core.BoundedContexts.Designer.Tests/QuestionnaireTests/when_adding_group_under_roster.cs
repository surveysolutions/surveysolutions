﻿using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests
{
    internal class when_adding_group_under_roster : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(new NewGroupAdded { PublicKey = parentRosterId, ParentGroupPublicKey = chapterId });
            questionnaire.Apply(new GroupBecameARoster(responsibleId, parentRosterId));

            eventContext = new EventContext();
        };

        Because of = () => questionnaire.AddGroup(groupId: groupId, responsibleId: responsibleId, title: title, condition: condition,
                    description: description, parentGroupId: parentRosterId, isRoster: false,
                    rosterSizeSource: RosterSizeSourceType.Question, rosterSizeQuestionId: null, rosterFixedTitles: null,
                    rosterTitleQuestionId: null);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_NewGroupAdded_event = () =>
            eventContext.ShouldContainEvent<NewGroupAdded>();

        It should_raise_NewGroupAdded_event_with_GroupId_specified = () =>
            eventContext.GetSingleEvent<NewGroupAdded>()
                .PublicKey.ShouldEqual(groupId);

        It should_raise_NewGroupAdded_event_with_ParentGroupId_specified = () =>
            eventContext.GetSingleEvent<NewGroupAdded>()
                .ParentGroupPublicKey.ShouldEqual(parentRosterId);

        It should_raise_NewGroupAdded_event_with_ConditionExpression_specified = () =>
            eventContext.GetSingleEvent<NewGroupAdded>()
                .ConditionExpression.ShouldEqual(condition);

        It should_raise_NewGroupAdded_event_with_Title_specified = () =>
            eventContext.GetSingleEvent<NewGroupAdded>()
                .GroupText.ShouldEqual(title);

        It should_raise_NewGroupAdded_event_with_Description_specified = () =>
            eventContext.GetSingleEvent<NewGroupAdded>()
                .Description.ShouldEqual(description);

        It should_raise_NewGroupAdded_event_with_ResponsibleId_specified = () =>
            eventContext.GetSingleEvent<NewGroupAdded>()
                .ResponsibleId.ShouldEqual(responsibleId);

        private static Questionnaire questionnaire;
        private static Guid groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static Guid parentRosterId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static string condition = "some condition";
        private static string title = "title";
        private static string description = "description";
        private static EventContext eventContext;
    }
}