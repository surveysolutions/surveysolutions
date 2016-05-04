using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionDataParserTests
{
    internal class when_building_answer_from_string_array_for_textlist_question : QuestionDataParserTestContext
    {
        private Establish context = () =>
        {
            questionDataParser = CreateQuestionDataParser();
            question = new TextListQuestion()
            {
                MaxAnswerCount = 5,
                PublicKey = questionId,
                QuestionType = QuestionType.TextList,
                StataExportCaption = questionVarName
            };
        };

        private Because of =
            () =>
                result =
                    questionDataParser.BuildAnswerFromStringArray(new[] { new Tuple<string, string>(questionVarName + "__1", "a"), new Tuple<string, string>(questionVarName + "__2", "b"), new Tuple<string, string>(questionVarName + "__3", "c") },
                        question);

        private It should_result_be_type_of_array_of_Tuple_decimal_string = () =>
            result.ShouldBeOfExactType<Tuple<decimal, string>[]>();

        private It should_result_has_3_answers = () =>
            ((Tuple<decimal, string>[]) result).Length.ShouldEqual(3);

        private It should_result_first_item_key_equal_to_1 = () =>
            ((Tuple<decimal, string>[]) result)[0].Item1.ShouldEqual(1);

        private It should_result_second_item_key_equal_to_2 = () =>
            ((Tuple<decimal, string>[]) result)[1].Item1.ShouldEqual(2);

        private It should_result_first_item_value_equal_to_1 = () =>
            ((Tuple<decimal, string>[]) result)[0].Item2.ShouldEqual("a");

        private It should_result_second_item_value_equal_to_2 = () =>
            ((Tuple<decimal, string>[]) result)[1].Item2.ShouldEqual("b");
    }
}