using FluentAssertions;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionDataParserTests
{
    internal class when_pasing_answer_on_text_question : QuestionDataParserTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            answer = "some answer";
            questionDataParser = CreateQuestionDataParser();
            question = new TextQuestion()
            {
                PublicKey = questionId,
                QuestionType = QuestionType.Text,
                StataExportCaption = questionVarName
            };
            BecauseOf();
        }

        public void BecauseOf() =>
            parsingResult = questionDataParser.TryParse(answer, questionVarName, question, out parcedValue, out parsedSingleColumnAnswer);

        [NUnit.Framework.Test] public void should_result_value_be_equal_to_answer () =>
            parcedValue.Should().Be(answer);
    }
}
