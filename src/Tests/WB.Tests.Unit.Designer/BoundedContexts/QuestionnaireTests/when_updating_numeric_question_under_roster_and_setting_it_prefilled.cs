using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_updating_numeric_question_under_roster_and_setting_it_prefilled : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            var chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            rosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionId = Guid.Parse("11111111111111111111111111111111");
            isPrefilled = true;

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = rosterId, ParentGroupPublicKey = chapterId });
            questionnaire.MarkGroupAsRoster(new GroupBecameARoster(responsibleId, rosterId));
            questionnaire.AddQuestion(Create.Event.NewQuestionAdded(publicKey: questionId, groupPublicKey: rosterId, questionType: QuestionType.Text));
            
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.UpdateNumericQuestion(questionId, "title",
                    "var1",null, isPrefilled, QuestionScope.Interviewer, null, false, null, properties: Create.QuestionProperties(), responsibleId: responsibleId, isInteger: false, countOfDecimalPlaces: null, validationConditions: new List<ValidationCondition>()));

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
    }
}