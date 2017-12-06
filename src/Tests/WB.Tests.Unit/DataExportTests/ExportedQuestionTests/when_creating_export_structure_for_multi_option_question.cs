﻿using Machine.Specifications;
using Main.Core.Entities.SubEntities;

namespace WB.Tests.Unit.DataExportTests.ExportedQuestionTests
{
    public class when_creating_export_structure_for_multi_option_question : ExportedQuestionTestContext
    {
        Establish context = () => { };

        Because of = () =>
        {
            filledQuestion = CreateFilledExportedQuestion(QuestionType.MultyOption, 3, new [] {2, 0});
            disabledQuestion = CreateDisabledExportedQuestion(QuestionType.MultyOption, columnsCount: 3);
            missingQuestion = CreateMissingValueExportedQuestion(QuestionType.MultyOption, columnsCount: 3);
        };

        It should_return_correct_filled_answer = () => filledQuestion.ShouldEqual(new []{ "1", "0", "1" });
        It should_return_correct_disabled_answer = () => disabledQuestion.ShouldEqual(new []{ DisableValue, DisableValue, DisableValue });
        It should_return_correct_missing_answer = () => missingQuestion.ShouldEqual(new []{ MissingNumericQuestionValue, MissingNumericQuestionValue, MissingNumericQuestionValue });


        private static string[] filledQuestion;
        private static string[] disabledQuestion;
        private static string[] missingQuestion;
    }
}