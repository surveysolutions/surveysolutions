using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.QuestionDataParserTests
{
    internal class when_pasing_answer_on_text_question : QuestionDataParserTestContext
    {
        private Establish context = () =>
        {
            answer = "some answer";
            questionDataParser = CreateQuestionDataParser();
        };

        private Because of =
            () =>
                parsingResult =
                    questionDataParser.TryParse(answer, 
                        new TextQuestion()
                        {
                            PublicKey = questionId,
                            QuestionType = QuestionType.Text,
                            StataExportCaption = questionVarName
                        },
                        out parcedValue);

        private It should_result_value_be_equal_to_answer = () =>
            parcedValue.Value.ShouldEqual(answer);

        private It should_result_key_be_equal_to_questionId = () =>
            parcedValue.Key.ShouldEqual(questionId);
    }
}
