using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.QuestionDataParserTests
{
    internal class when_building_answer_from_string_array_for_numeric_qustion : QuestionDataParserTestContext
    {
        private Establish context = () =>
        {
            answer = "1";
            questionDataParser = CreateQuestionDataParser();
        };

        private Because of =
            () =>
                result =
                    questionDataParser.BuildAnswerFromStringArray(new string[] { answer }, new NumericQuestion()
                        {
                            PublicKey = questionId,
                            QuestionType = QuestionType.Numeric,
                            IsInteger = true,
                            StataExportCaption = questionVarName
                        });

        private It should_result_be_equal_to_1 = () =>
            result.Value.Value.ShouldEqual(1);

        private It should_result_key_be_equal_to_questionId = () =>
            result.Value.Key.ShouldEqual(questionId);
    }
}