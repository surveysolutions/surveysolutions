using FluentAssertions;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionDataParserTests
{
    internal class when_pasing_answer_on_nonexistent_question : QuestionDataParserTestContext
    {
        [NUnit.Framework.OneTimeSetUp]
        public void context()
        {
            questionDataParser = CreateQuestionDataParser();
            BecauseOf();
        }

        private void BecauseOf() => parsingResult = questionDataParser.TryParse("some answer", "tt", null, out parcedValue, out parsedSingleColumnAnswer);

        [NUnit.Framework.Test] public void should_result_be_QuestionWasNotFound () =>
            parsingResult.Should().Be(ValueParsingResult.QuestionWasNotFound);
    }
}
