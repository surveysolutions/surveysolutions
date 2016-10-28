using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionDataParserTests
{
    internal class when_pasing_answer_on_text_question : QuestionDataParserTestContext
    {
        Establish context = () =>
        {
            answer = "some answer";
            questionDataParser = CreateQuestionDataParser();
            question = new TextQuestion()
            {
                PublicKey = questionId,
                QuestionType = QuestionType.Text,
                StataExportCaption = questionVarName
            };
        };

        Because of = () =>
            parsingResult = questionDataParser.TryParse(answer, questionVarName, question, out parcedValue, out parsedSingleColumnAnswer);

        It should_result_value_be_equal_to_answer = () =>
            parcedValue.ShouldEqual(answer);
    }
}
