using FluentAssertions;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionDataParserTests
{
    internal class when_pasing_answer_on_numeric_int_question : QuestionDataParserTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            answer = "1";
            questionDataParser = CreateQuestionDataParser();
            question = new NumericQuestion()
            {
                PublicKey = questionId,
                QuestionType = QuestionType.Numeric,
                IsInteger = true,
                StataExportCaption = questionVarName
            };
            BecauseOf();
        }

        private void BecauseOf() =>
                parsingResult =
                    questionDataParser.TryParse(answer, questionVarName, question, out parcedValue, out parsedSingleColumnAnswer);

        [NUnit.Framework.Test] public void should_result_be_equal_to_1 () =>
            parcedValue.Should().Be(1);
    }
}
