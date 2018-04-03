using FluentAssertions;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionDataParserTests
{
    internal class when_parsing_empty_string_as_an_answer : QuestionDataParserTestContext
    {
        [NUnit.Framework.OneTimeSetUp]
        public void context()
        {
            questionDataParser = CreateQuestionDataParser();
            BecauseOf();
        }

        private void BecauseOf() => parsingResult = questionDataParser.TryParse(string.Empty,"va", null, out parcedValue, out parsedSingleColumnAnswer);

        [NUnit.Framework.Test] public void should_result_be_ValueIsNullOrEmpty () =>
            parsingResult.Should().Be(ValueParsingResult.ValueIsNullOrEmpty);
    }
}
