using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.QuestionDataParserTests
{
    internal class when_pasing_answer_on_autopropagated_question : QuestionDataParserTestContext
    {
        private Establish context = () =>
        {
            answer = "1";
            questionDataParser = CreateQuestionDataParser();
        };

        private Because of =
            () =>
                parsingResult =
                    questionDataParser.TryParse(answer, new AutoPropagateQuestion()
                    {
                        PublicKey = questionId,
                        QuestionType = QuestionType.AutoPropagate,
                        StataExportCaption = questionVarName
                    }, out parcedValue);

        private It should_result_be_equal_to_1 = () =>
            parcedValue.Value.ShouldEqual(1);

        private It should_result_key_be_equal_to_questionId = () =>
            parcedValue.Key.ShouldEqual(questionId);
    }
}