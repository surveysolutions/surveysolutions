﻿using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateNumericQuestionHandlerTests
{
    internal class when_updating_numeric_question_with_title_which_contains_roster_title_as_substitution_reference : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            questionId = Guid.Parse("11111111111111111111111111111111");

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });

            questionnaire.Apply(Create.Event.NumericQuestionAdded(
                groupPublicKey: chapterId,
                publicKey: questionId,
                stataExportCaption: "var",
                questionText: "title"
            ));
            eventContext = new EventContext();
        };

        Because of = () => exception = Catch.Exception(() => questionnaire.UpdateNumericQuestion(questionId, questionTitle, "var",null, false, QuestionScope.Interviewer, null, false, null, 
                responsibleId: responsibleId, isInteger: false, countOfDecimalPlaces: null, validationConditions: new List<ValidationCondition>(), properties: new QuestionProperties(false)));

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__unknown__and__substitution__ = () =>
            new[] { "unknown", "substitution" }.ShouldEachConformTo(
           keyword => exception.Message.ToLower().Contains(keyword));

        private static EventContext eventContext;
        private static Questionnaire questionnaire;
        private static Guid questionId;
        private static Guid chapterId;
        private static Guid responsibleId;
        private static string questionTitle = "title %rostertitle%";
        private static Exception exception;
    }
}
