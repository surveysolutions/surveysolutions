﻿using Machine.Specifications;
using Main.Core.Entities.SubEntities;

namespace WB.Tests.Unit.DataExportTests.ExportedQuestionTests
{
    public class when_creating_export_structure_for_text_question : ExportedQuestionTestContext
    {
        Establish context = () => { };

        Because of = () =>
        {
            filledQuestion = CreateFilledExportedQuestion(QuestionType.Text, "filled");
            disabledQuestion = CreateDisabledExportedQuestion(QuestionType.Text);
            missingQuestion = CreateMissingValueExportedQuestion(QuestionType.Text);
        };

        It should_return_correct_filled_answer = () => filledQuestion.ShouldEqual(new []{ "filled"});
        It should_return_correct_disabled_answer = () => disabledQuestion.ShouldEqual(new []{ DisableQuestionValue });
        It should_return_correct_missing_answer = () => missingQuestion.ShouldEqual(new []{ MissingStringQuestionValue });


        private static string[] filledQuestion;
        private static string[] disabledQuestion;
        private static string[] missingQuestion;
    }
}