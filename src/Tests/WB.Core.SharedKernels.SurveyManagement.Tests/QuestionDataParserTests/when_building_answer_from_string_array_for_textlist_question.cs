using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.QuestionDataParserTests
{
    internal class when_building_answer_from_string_array_for_textlist_question : QuestionDataParserTestContext
    {
        Establish context = () => { questionDataParser = CreateQuestionDataParser(); };

        Because of =
            () => result = questionDataParser.BuildAnswerFromStringArray(new string[] { "a","b","c" }, questionVarName, CreateQuestionnaireDocumentWithOneChapter(new TextListQuestion() {MaxAnswerCount = 5, PublicKey = questionId, QuestionType = QuestionType.TextList, StataExportCaption = questionVarName }));

        It should_result_be_type_of_array_of_Tuple_decimal_string = () =>
            result.Value.Value.ShouldBeOfExactType<Tuple<decimal, string>[]>();

        It should_result_has_3_answers = () =>
            ((Tuple<decimal, string>[])result.Value.Value).Length.ShouldEqual(3);

        It should_result_first_item_key_equal_to_1 = () =>
            ((Tuple<decimal, string>[])result.Value.Value)[0].Item1.ShouldEqual(1);

        It should_result_second_item_key_equal_to_2 = () =>
           ((Tuple<decimal, string>[])result.Value.Value)[1].Item1.ShouldEqual(2);

        It should_result_first_item_value_equal_to_1 = () =>
            ((Tuple<decimal, string>[])result.Value.Value)[0].Item2.ShouldEqual("a");

        It should_result_second_item_value_equal_to_2 = () =>
           ((Tuple<decimal, string>[])result.Value.Value)[1].Item2.ShouldEqual("b");

        It should_result_key_be_equal_to_questionId = () =>
            result.Value.Key.ShouldEqual(questionId);
    }
}
