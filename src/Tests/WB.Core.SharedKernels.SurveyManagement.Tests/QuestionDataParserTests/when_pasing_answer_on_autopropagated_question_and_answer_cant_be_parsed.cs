using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.QuestionDataParserTests
{
    internal class when_pasing_answer_on_autopropagated_question_and_answer_cant_be_parsed : QuestionDataParserTestContext
    {
        private Establish context = () =>
        {
            answer = "unparsed";
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

        private It should_result_be_AnswerAsIntWasNotParsed = () =>
            parsingResult.ShouldEqual(ValueParsingResult.AnswerAsIntWasNotParsed);
    }
}