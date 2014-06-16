using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.QuestionDataParserTests
{
    internal class when_building_answer_from_string_array_for_multy_option_qustion : QuestionDataParserTestContext
    {
        private Establish context = () => { questionDataParser = CreateQuestionDataParser(); };

        private Because of =
            () =>
                result =
                    questionDataParser.BuildAnswerFromStringArray(new string[] { "1", "2" },
                        new MultyOptionsQuestion()
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
                        });

        private It should_result_be_type_of_array_of_decimal = () =>
            result.Value.Value.ShouldBeOfExactType<decimal[]>();

        private It should_result_has_2_answers = () =>
            ((decimal[]) result.Value.Value).Length.ShouldEqual(2);

        private It should_result_first_item_equal_to_1 = () =>
            ((decimal[]) result.Value.Value)[0].ShouldEqual(1);

        private It should_result_second_item_equal_to_2 = () =>
            ((decimal[]) result.Value.Value)[1].ShouldEqual(2);

        private It should_result_key_be_equal_to_questionId = () =>
            result.Value.Key.ShouldEqual(questionId);
    }
}