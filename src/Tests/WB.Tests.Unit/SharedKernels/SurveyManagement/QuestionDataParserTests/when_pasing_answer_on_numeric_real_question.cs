using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionDataParserTests
{
    internal class when_pasing_answer_on_numeric_real_question : QuestionDataParserTestContext
    {
        Establish context = () =>
        {
            answer = "1.22";
            questionDataParser = CreateQuestionDataParser();
            question = new NumericQuestion()
            {
                PublicKey = questionId,
                QuestionType = QuestionType.Numeric,
                IsInteger = false,
                StataExportCaption = questionVarName
            };
        };

        Because of = () =>
            parsingResult = questionDataParser.TryParse(answer, questionVarName, question, out parcedValue, out parsedSingleColumnAnswer);

        It should_result_be_equal_to_1_22 = () =>
            parcedValue.ShouldEqual((decimal) 1.22);
    }
}