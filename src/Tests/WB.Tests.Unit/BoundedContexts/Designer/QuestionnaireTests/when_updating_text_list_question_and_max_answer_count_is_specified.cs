using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests
{
    internal class when_updating_text_list_question_and_max_answer_count_is_specified : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            maxAnswerCountValue = 42;

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(new NewQuestionAdded { PublicKey = questionId, QuestionType = QuestionType.Text });
        };

        Because of = () =>
            exception = Catch.Exception(() => questionnaire.UpdateTextListQuestion(questionId, "title", "var1", null, false, null, null, null, null, responsibleId, maxAnswerCountValue));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__maximum__ = () =>
            exception.Message.ToLower().ShouldContain("maximum");

        It should_throw_exception_with_message_containting__answers__ = () =>
            exception.Message.ToLower().ShouldContain("answers");

        It should_throw_exception_with_message_containting__from_1_to_40__ = () =>
            exception.Message.ToLower().ShouldContain("from 1 to 40");

        private static Exception exception;
        private static int maxAnswerCountValue;
        private static Questionnaire questionnaire;
        private static Guid questionId= Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
    }
}