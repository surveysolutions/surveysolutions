using System;
using System.Collections.Generic;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateMultiOptionQuestionHandlerTests
{
    internal class when_updating_multi_option_question_and_more_than_one_question_with_same_id_already_exists : QuestionnaireTestsContext
    {
        [NUnit.Framework.Test] public void should_throw_QuestionnaireException () {
            var answers = new List<Answer>() { new Answer() { AnswerCode = 1, AnswerText = "1" }, new Answer() { AnswerCode = 2, AnswerText = "2" } };

            questionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            var questionnaireDoc = Create.QuestionnaireDocument(
                children: new List<IComposite>
                {
                    Create.SingleQuestion(id:questionId,isFilteredCombobox:false, options:answers),
                    Create.SingleQuestion(id:questionId,isFilteredCombobox:false, options:answers)
                });

            questionnaire = Create.Questionnaire();
            questionnaire.Initialize(Guid.NewGuid(),questionnaireDoc,null);

            exception = Assert.Throws<QuestionnaireException>(() =>
                questionnaire.UpdateMultiOptionQuestion(
                    Create.Command.UpdateMultiOptionQuestion(
                        questionId,
                        responsibleId,
                        title,
                        variableName)));
            exception.Message.ToLower().ToSeparateWords().Should().Contain(new[] { "more", "question(s)", "exist" });

        }

        private static Questionnaire questionnaire;
        private static Exception exception;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static string variableName = "qr_barcode_question";
        private static string title = "title";
    }
}
