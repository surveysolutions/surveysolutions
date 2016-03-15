﻿using System;
using Machine.Specifications;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_moving_numeric_question_with_title_which_contains_roster_title_as_substitution_reference_from_roster_to_group: QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            questionId = Guid.Parse("11111111111111111111111111111111");
            rosterId = Guid.Parse("21111111111111111111111111111111");
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(new NewGroupAdded { PublicKey = rosterId, ParentGroupPublicKey = chapterId });
            questionnaire.Apply(new GroupBecameARoster(Guid.NewGuid(), rosterId));
            questionnaire.Apply(Create.Event.NumericQuestionAdded(
                publicKey : questionId,
                groupPublicKey : rosterId,
                questionText : questionTitle,
                stataExportCaption : "var"
            ));
            eventContext = new EventContext();
        };

        Because of = () => exception = Catch.Exception(() => questionnaire.MoveQuestion(questionId, chapterId, 1, responsibleId));

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
        private static Guid rosterId;
        private static Guid responsibleId;
        private static Exception exception;
        private static string questionTitle = "title %rostertitle%";
    }
}
