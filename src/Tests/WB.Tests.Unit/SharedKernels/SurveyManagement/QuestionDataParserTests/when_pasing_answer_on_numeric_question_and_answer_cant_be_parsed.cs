using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionDataParserTests
{
    internal class when_pasing_answer_on_numeric_question_and_answer_cant_be_parsed : QuestionDataParserTestContext
    {
        private Establish context = () =>
        {
            answer = "unparsed";
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

        private It should_result_be_AnswerAsIntWasNotParsed = () =>
            parsingResult.ShouldEqual(ValueParsingResult.AnswerAsIntWasNotParsed);
    }
}