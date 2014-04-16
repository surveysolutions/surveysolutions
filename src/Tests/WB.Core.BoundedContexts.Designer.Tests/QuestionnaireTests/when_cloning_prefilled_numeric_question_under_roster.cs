﻿using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests
{
    internal class when_cloning_prefilled_numeric_question_under_roster : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            var chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            rosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            sourceQuestionId = Guid.Parse("44444444444444444444444444444444");
            questionId = Guid.Parse("11111111111111111111111111111111");
            isPrefilled = true;

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(new NewQuestionAdded { PublicKey = sourceQuestionId, QuestionType = QuestionType.Text, GroupPublicKey = chapterId });
            questionnaire.Apply(new NewGroupAdded { PublicKey = rosterId });
            questionnaire.Apply(new GroupBecameARoster(responsibleId, rosterId));
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.CloneNumericQuestion(questionId, rosterId, "title",
                    "var1", false, isPrefilled, QuestionScope.Interviewer, null, null, null, null,
                    sourceQuestionId, 0, responsibleId, isInteger: false, countOfDecimalPlaces: null,
                    maxValue: null));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__prefilled__ = () =>
            exception.Message.ToLower().ShouldContain("pre-filled");

        It should_throw_exception_with_message_containting__roster__ = () =>
            exception.Message.ToLower().ShouldContain("roster");

        private static Exception exception;
        private static Questionnaire questionnaire;
        private static Guid questionId;
        private static Guid rosterId;
        private static Guid responsibleId;
        private static bool isPrefilled;
        private static Guid sourceQuestionId;
    }
}