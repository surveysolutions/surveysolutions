using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.QuestionDataParserTests
{
    internal class when_pasing_answer_on_numeric_real_question_and_answer_is_greater_then_allowed : QuestionDataParserTestContext
    {
        private Establish context = () =>
        {
            answer = "5.6";
            questionDataParser = CreateQuestionDataParser();
            question = new NumericQuestion()
            {
                PublicKey = questionId,
                QuestionType = QuestionType.Numeric,
                IsInteger = false,
                MaxValue = 3,
                StataExportCaption = questionVarName
            };
        };

        private Because of =
            () =>
                parsingResult =
                    questionDataParser.TryParse(answer, question, CreateQuestionnaireDocumentWithOneChapter(question), out parcedValue);

        private It should_result_be_equal_to_5_6 = () =>
            parcedValue.Value.ShouldEqual((decimal) 5.6);

        private It should_result_key_be_equal_to_questionId = () =>
            parcedValue.Key.ShouldEqual(questionId);

        private It should_parsing_result_be_equal_to_AnswerIsIncorrectBecauseIsGreaterThanMaxValue = () =>
            parsingResult.ShouldEqual(ValueParsingResult.AnswerIsIncorrectBecauseIsGreaterThanMaxValue);
    }
}