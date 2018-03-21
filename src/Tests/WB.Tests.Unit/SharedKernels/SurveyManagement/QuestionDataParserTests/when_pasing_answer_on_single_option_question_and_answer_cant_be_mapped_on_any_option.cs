using FluentAssertions;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionDataParserTests
{
    internal class when_pasing_answer_on_single_option_question_and_answer_cant_be_mapped_on_any_option : QuestionDataParserTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            answer = "1";
            questionDataParser = CreateQuestionDataParser();
            question = new SingleQuestion()
            {
                PublicKey = questionId,
                QuestionType = QuestionType.SingleOption,
                StataExportCaption = questionVarName
            };
            BecauseOf();
        }

        private void BecauseOf() =>
                parsingResult =
                    questionDataParser.TryParse(answer, questionVarName, question, out parcedValue, out parsedSingleColumnAnswer);

        [NUnit.Framework.Test] public void should_result_be_ParsedValueIsNotAllowed () =>
            parsingResult.Should().Be(ValueParsingResult.ParsedValueIsNotAllowed);
    }
}
