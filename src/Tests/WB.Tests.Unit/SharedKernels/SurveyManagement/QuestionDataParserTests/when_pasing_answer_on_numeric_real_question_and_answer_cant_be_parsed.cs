using FluentAssertions;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionDataParserTests
{
    internal class when_pasing_answer_on_numeric_real_question_and_answer_cant_be_parsed : QuestionDataParserTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            answer = "unparsed";
            questionDataParser = CreateQuestionDataParser();
            question = new NumericQuestion()
            {
                PublicKey = questionId,
                QuestionType = QuestionType.Numeric,
                IsInteger = false,
                StataExportCaption = questionVarName
            };
            BecauseOf();
        }

        private void BecauseOf() =>
                parsingResult =
                    questionDataParser.TryParse(answer, questionVarName, question, out parcedValue, out parsedSingleColumnAnswer);

        [NUnit.Framework.Test] public void should_result_be_AnswerAsDecimalWasNotParsed () =>
            parsingResult.Should().Be(ValueParsingResult.AnswerAsDecimalWasNotParsed);
    }
}
