using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.QuestionDataParserTests
{
    internal class when_pasing_answer_on_numeric_real_question : QuestionDataParserTestContext
    {
        private Establish context = () =>
        {
            answer = "1.22";
            questionDataParser = CreateQuestionDataParser();
        };

        private Because of =
            () =>
                parsingResult =
                    questionDataParser.TryParse(answer, new NumericQuestion()
                    {
                        PublicKey = questionId,
                        QuestionType = QuestionType.Numeric,
                        IsInteger = false,
                        StataExportCaption = questionVarName
                    },
                        out parcedValue);

        private It should_result_be_equal_to_1_22 = () =>
            parcedValue.Value.ShouldEqual((decimal) 1.22);

        private It should_result_key_be_equal_to_questionId = () =>
            parcedValue.Key.ShouldEqual(questionId);
    }
}