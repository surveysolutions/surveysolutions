using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.QuestionDataParserTests
{
    internal class when_pasing_answer_on_numeric_int_question : QuestionDataParserTestContext
    {
        private Establish context = () =>
        {
            answer = "1";
            questionDataParser = CreateQuestionDataParser();
        };

        private Because of =
            () =>
                parsingResult =
                    questionDataParser.TryParse(answer, new NumericQuestion()
                    {
                        PublicKey = questionId,
                        QuestionType = QuestionType.Numeric,
                        IsInteger = true,
                        StataExportCaption = questionVarName
                    }, out parcedValue);

        private It should_result_be_equal_to_1 = () =>
            parcedValue.Value.ShouldEqual(1);

        private It should_result_key_be_equal_to_questionId = () =>
            parcedValue.Key.ShouldEqual(questionId);
    }
}