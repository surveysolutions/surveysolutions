using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using NHibernate.Util;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionDataParserTests
{
    internal class when_building_answer_from_string_array_for_textlist_question : QuestionDataParserTestContext
    {
        Establish context = () =>
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

        Because of = () =>
            result = questionDataParser.BuildAnswerFromStringArray(new[] { new Tuple<string, string>(questionVarName + "__1", "a"), new Tuple<string, string>(questionVarName + "__2", "b"), new Tuple<string, string>(questionVarName + "__3", "c") }, question);

        It should_result_be_type_of_array_of_Tuple_decimal_string = () =>
            result.ShouldBeOfExactType<TextListAnswer>();

        It should_result_has_3_answers = () =>
            ((TextListAnswer) result).Rows.Count.ShouldEqual(3);

        It should_result_first_item_key_equal_to_1 = () =>
            ((TextListAnswer) result).Rows.First().Value.ShouldEqual(1);

        It should_result_second_item_key_equal_to_2 = () =>
            ((TextListAnswer) result).Rows.Second().Value.ShouldEqual(2);

        It should_result_first_item_value_equal_to_1 = () =>
            ((TextListAnswer) result).Rows.First().Text.ShouldEqual("a");

        It should_result_second_item_value_equal_to_2 = () =>
            ((TextListAnswer) result).Rows.Second().Text.ShouldEqual("b");
    }
}