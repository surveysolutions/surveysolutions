using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.QuestionDataParserTests
{
    internal class when_pasing_answer_on_multy_option_question_and_answer_cant_is_not_decimal : QuestionDataParserTestContext
    {
        private Establish context = () =>
        {
            answer = "unparsed";
            questionDataParser = CreateQuestionDataParser();
        };

        private Because of =
            () =>
                parsingResult =
                    questionDataParser.TryParse(answer, new MultyOptionsQuestion()
                    {
                        PublicKey = questionId,
                        QuestionType = QuestionType.MultyOption,
                        StataExportCaption = questionVarName
                    }, out parcedValue);

        private It should_result_be_AnswerAsDecimalWasNotParsed = () =>
            parsingResult.ShouldEqual(ValueParsingResult.AnswerAsDecimalWasNotParsed);
    }
}