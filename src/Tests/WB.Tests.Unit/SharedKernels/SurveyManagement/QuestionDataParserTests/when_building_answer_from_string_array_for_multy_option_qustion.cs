using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionDataParserTests
{
    internal class when_building_answer_from_string_array_for_multy_option_qustion : QuestionDataParserTestContext
    {
        private Establish context = () =>
        {
            questionDataParser = CreateQuestionDataParser();
            question = new MultyOptionsQuestion()
            {
                PublicKey = questionId,
                QuestionType = QuestionType.MultyOption,
                Answers =
                    new List<Answer>()
                    {
                        new Answer() { AnswerValue = "1", AnswerText = "a" },
                        new Answer() { AnswerValue = "2", AnswerText = "b" },
                        new Answer() { AnswerValue = "3", AnswerText = "c" }
                    },
                StataExportCaption = questionVarName
            };
        };

        private Because of =
            () =>
                result =
                    questionDataParser.BuildAnswerFromStringArray(new [] { new Tuple<string, string>(questionVarName + "__1", "1"), new Tuple<string, string>(questionVarName + "__2", "2") },
                        question);

        private It should_result_be_type_of_array_of_decimal = () =>
            result.ShouldBeOfExactType<decimal[]>();

        private It should_result_has_2_answers = () =>
            ((decimal[]) result).Length.ShouldEqual(2);

        private It should_result_first_item_equal_to_1 = () =>
            ((decimal[]) result)[0].ShouldEqual(1);

        private It should_result_second_item_equal_to_2 = () =>
            ((decimal[]) result)[1].ShouldEqual(2);
    }
}