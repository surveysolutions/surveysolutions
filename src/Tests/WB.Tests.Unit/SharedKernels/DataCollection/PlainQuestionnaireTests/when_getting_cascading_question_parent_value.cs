using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Tests.Unit.SharedKernels.DataCollection.PlainQuestionnaireTests
{
    internal class when_getting_cascading_question_parent_value : PlainQuestionnaireTestsContext
    {
        Establish context = () =>
        {
            var questionnaireDocument = new QuestionnaireDocument();
            var version = 0;

            var childQuestion = CreateSingleOptionQuestion(questionId, new[] { new Answer { AnswerValue = "1", ParentValue = "1" } });

            questionnaireDocument.Add(childQuestion, null, null);

            plainQuestionnaire = new PlainQuestionnaire(questionnaireDocument, version);
        };

        Because of = () =>
            result = plainQuestionnaire.GetCascadingParentValue(questionId, 1m);

        It result_should_be_equal_1 = () =>
            result.ShouldEqual(1m);

        private static PlainQuestionnaire plainQuestionnaire;
        private static decimal result;
        private static readonly Guid questionId = Guid.Parse("00000000000000000000000000000000");
    }
}