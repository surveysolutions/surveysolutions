using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionDataParserTests
{
    internal class when_parsing_empty_string_as_an_answer : QuestionDataParserTestContext
    {
        Establish context = () => { questionDataParser = CreateQuestionDataParser(); };

        Because of =
            () => parsingResult = questionDataParser.TryParse(string.Empty,"va", null, out parcedValue, out parsedSingleColumnAnswer);

        It should_result_be_ValueIsNullOrEmpty = () =>
            parsingResult.ShouldEqual(ValueParsingResult.ValueIsNullOrEmpty);
    }
}