﻿using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireTests
{
    internal class when_moving_roster_size_question_to_deeper_level : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            rosterGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            targetRosterGroupId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");

            rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });

            questionnaire.Apply(Create.Event.NumericQuestionAdded
            (
                publicKey : rosterSizeQuestionId,
                isInteger : true,
                groupPublicKey : chapterId
            ));

            AddGroup(questionnaire: questionnaire, groupId: rosterGroupId, parentGroupId: chapterId, condition: null,
                responsibleId: responsibleId, rosterSizeQuestionId: rosterSizeQuestionId, isRoster: true);

            AddGroup(questionnaire: questionnaire, groupId: targetRosterGroupId, parentGroupId: chapterId, condition: null,
                responsibleId: responsibleId, rosterSizeQuestionId: null, isRoster: true, rosterSizeSource: RosterSizeSourceType.FixedTitles,
                rosterTitleQuestionId: null, rosterFixedTitles: new[] { new FixedRosterTitleItem("1", "fixed title 1"), new FixedRosterTitleItem("2", "test 2") });
        };

        private Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.MoveQuestion(rosterSizeQuestionId, targetRosterGroupId, targetIndex: 0, responsibleId: responsibleId));

        private It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        private It should_throw_exception_with_message_containting__title__ = () =>
            exception.Message.ToLower().ShouldContain("source");

        private It should_throw_exception_with_message_containting__question__ = () =>
            exception.Message.ToLower().ShouldContain("question");

        private It should_throw_exception_with_message_containting__roster__ = () =>
            exception.Message.ToLower().ShouldContain("roster");

        private static Exception exception;
        private static Questionnaire questionnaire;
        private static Guid responsibleId;
        private static Guid rosterGroupId;
        private static Guid targetRosterGroupId;
        private static Guid chapterId;
        private static Guid rosterSizeQuestionId;
    }
}

