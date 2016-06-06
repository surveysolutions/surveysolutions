using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionDataParserTests
{
    internal class when_pasing_answer_on_datetime_question_and_answer_has_time : QuestionDataParserTestContext
    {
        Establish context = () =>
        {
            answer = "2016-01-01T12:12:31";
            questionDataParser = CreateQuestionDataParser();
            question = new DateTimeQuestion()
            {
                PublicKey = questionId,
                QuestionType = QuestionType.DateTime,
                StataExportCaption = questionVarName
            };
        };

        Because of = () => parsingResult = questionDataParser.TryParse(answer, questionVarName, question, out parcedValue);

        It should_result_be_AnswerAsDateTimeWasNotParsed = () =>
            parsingResult.ShouldEqual(ValueParsingResult.AnswerAsDateTimeWasNotParsed);
    }
}