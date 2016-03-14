﻿using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireTests
{
    internal class when_updating_roster_group_by_numeric_question_and_roster_title_inside_group_by_roster_size : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            var chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            parentGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            groupId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            rosterTitleQuestionId = Guid.Parse("1BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            rosterSizeQuestionId = Guid.Parse("2BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            rosterGroupWithRosterTitleQuestionId = Guid.Parse("3BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            rosterSizeSourceType = RosterSizeSourceType.Question;

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(new NewGroupAdded { PublicKey = groupId, ParentGroupPublicKey = chapterId });
            questionnaire.Apply(new NewGroupAdded { PublicKey = rosterGroupWithRosterTitleQuestionId, ParentGroupPublicKey = chapterId });
            questionnaire.Apply(new GroupBecameARoster(responsibleId: responsibleId, groupId: rosterGroupWithRosterTitleQuestionId));
            questionnaire.Apply(new RosterChanged(responsibleId: responsibleId, groupId: rosterGroupWithRosterTitleQuestionId){
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    RosterSizeSource = RosterSizeSourceType.Question,
                    FixedRosterTitles =  null,
                    RosterTitleQuestionId =null 
                });

            questionnaire.Apply(Create.Event.NewQuestionAdded(publicKey: rosterTitleQuestionId, groupPublicKey: rosterGroupWithRosterTitleQuestionId, questionType: QuestionType.Text));
            
            questionnaire.Apply(Create.Event.NewQuestionAdded(publicKey: rosterSizeQuestionId, groupPublicKey: chapterId, questionType: QuestionType.Numeric, isInteger:true));

            questionnaire.Apply(new NewGroupAdded { PublicKey = parentGroupId });

            eventContext = new EventContext();
        };

        Because of = () =>
            questionnaire.UpdateGroup(groupId: groupId, responsibleId: responsibleId, title: "title", variableName: null,
                description: null, condition: null, hideIfDisabled: false, rosterSizeQuestionId: rosterSizeQuestionId,
                isRoster: true, rosterSizeSource: rosterSizeSourceType, rosterFixedTitles: null,
                rosterTitleQuestionId: rosterTitleQuestionId);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };


        It should_raise_GroupBecameARoster_event_with_GroupId_specified = () =>
            eventContext.GetSingleEvent<GroupBecameARoster>()
                .GroupId.ShouldEqual(groupId);

        It should_raise_RosterChanged_event_with_GroupId_specified = () =>
            eventContext.GetSingleEvent<RosterChanged>()
                .GroupId.ShouldEqual(groupId);

        It should_raise_RosterChanged_event_with_RosterSizeQuestionId_equal_to_specified_question_id = () =>
            eventContext.GetSingleEvent<RosterChanged>()
                .RosterSizeQuestionId.ShouldEqual(rosterSizeQuestionId);

        It should_raise_RosterChanged_event_with_RosterTitleQuestionId_equal_to_specified_question_id = () =>
            eventContext.GetSingleEvent<RosterChanged>()
                .RosterTitleQuestionId.ShouldEqual(rosterTitleQuestionId);

        private static EventContext eventContext;
        private static Questionnaire questionnaire;
        private static Guid responsibleId;
        private static Guid groupId;
        private static Guid rosterTitleQuestionId;
        private static Guid rosterSizeQuestionId;
        private static Guid parentGroupId;
        private static Guid rosterGroupWithRosterTitleQuestionId;
        private static RosterSizeSourceType rosterSizeSourceType;
    }
}