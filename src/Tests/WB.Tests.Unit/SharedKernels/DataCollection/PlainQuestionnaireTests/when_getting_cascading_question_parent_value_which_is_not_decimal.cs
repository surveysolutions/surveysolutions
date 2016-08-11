using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Tests.Unit.SharedKernels.DataCollection.PlainQuestionnaireTests
{
    internal class when_getting_cascading_question_parent_value_which_is_not_decimal : PlainQuestionnaireTestsContext
    {
        Establish context = () =>
        {
            var questionnaireDocument = new QuestionnaireDocument();
            var version = 0;

            var childQuestion = CreateSingleOptionQuestion(questionId, new[] { new Answer { AnswerValue = "1", ParentValue = "A" } });

            questionnaireDocument.Add(childQuestion, null, null);

            plainQuestionnaire = new PlainQuestionnaire(questionnaireDocument, version);
        };

        Because of = () =>
            exception = Catch.Exception(() => plainQuestionnaire.GetCascadingParentValue(questionId, 1m)
        );

        It should_throw_exception_type_of_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containing__parse____decimal = () =>
            exception.Message.ToLower().ToSeparateWords().ShouldContain("no", "parent");

        private static PlainQuestionnaire plainQuestionnaire;
        private static readonly Guid questionId = Guid.Parse("00000000000000000000000000000000");
        private static Exception exception;
    }
}