﻿using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_moving_roster_size_question_in_paralell_roster_scope: QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            rosterGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            targetRosterGroupId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");

            rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");

            var parallelRosterId = Guid.Parse("21111111111111111111111111111111");

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = chapterId });

            AddGroup(questionnaire: questionnaire, groupId: parallelRosterId, parentGroupId: chapterId, condition: null,
                responsibleId: responsibleId, rosterSizeQuestionId: null, isRoster: true, rosterSizeSource: RosterSizeSourceType.FixedTitles,
                rosterTitleQuestionId: null, rosterFixedTitles: new[] { new FixedRosterTitleItem("1", "fixed title 1"), new FixedRosterTitleItem("2", "test 2") });

            questionnaire.AddNumericQuestion(rosterSizeQuestionId, parallelRosterId, responsibleId, isInteger: true);

            AddGroup(questionnaire: questionnaire, groupId: rosterGroupId, parentGroupId: parallelRosterId, condition: null,
                responsibleId: responsibleId, rosterSizeQuestionId: rosterSizeQuestionId, isRoster: true);

            AddGroup(questionnaire: questionnaire, groupId: targetRosterGroupId, parentGroupId: chapterId, condition: null,
                responsibleId: responsibleId, rosterSizeQuestionId: null, isRoster: true, rosterSizeSource: RosterSizeSourceType.FixedTitles,
                rosterTitleQuestionId: null, rosterFixedTitles: new[] { new FixedRosterTitleItem("1", "fixed title 1"), new FixedRosterTitleItem("2", "test 2") });
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.MoveQuestion(rosterSizeQuestionId, targetRosterGroupId, targetIndex: 0, responsibleId: responsibleId));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__title__ = () =>
            exception.Message.ToLower().ShouldContain("source");

        It should_throw_exception_with_message_containting__question__ = () =>
            exception.Message.ToLower().ShouldContain("question");

        It should_throw_exception_with_message_containting__roster__ = () =>
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
