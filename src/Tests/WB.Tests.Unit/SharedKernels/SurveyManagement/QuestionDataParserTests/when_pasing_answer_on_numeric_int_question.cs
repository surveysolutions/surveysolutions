using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionDataParserTests
{
    internal class when_pasing_answer_on_numeric_int_question : QuestionDataParserTestContext
    {
        private Establish context = () =>
        {
            answer = "1";
            questionDataParser = CreateQuestionDataParser();
            question = new NumericQuestion()
            {
                PublicKey = questionId,
                QuestionType = QuestionType.Numeric,
                IsInteger = true,
                StataExportCaption = questionVarName
            };
        };

        private Because of =
            () =>
                parsingResult =
                    questionDataParser.TryParse(answer, questionVarName, question, CreateQuestionnaireDocumentWithOneChapter(question), out parcedValue);

        private It should_result_be_equal_to_1 = () =>
            parcedValue.Value.ShouldEqual(1);

        private It should_result_key_be_equal_to_questionId = () =>
            parcedValue.Key.ShouldEqual(questionId);
    }
}