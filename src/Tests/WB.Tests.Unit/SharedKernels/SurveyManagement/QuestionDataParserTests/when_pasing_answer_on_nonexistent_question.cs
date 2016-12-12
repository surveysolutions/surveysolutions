using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionDataParserTests
{
    internal class when_pasing_answer_on_nonexistent_question : QuestionDataParserTestContext
    {
        private Establish context = () => { questionDataParser = CreateQuestionDataParser(); };

        private Because of =
            () => parsingResult = questionDataParser.TryParse("some answer", "tt", null, out parcedValue, out parsedSingleColumnAnswer);

        private It should_result_be_QuestionWasNotFound = () =>
            parsingResult.ShouldEqual(ValueParsingResult.QuestionWasNotFound);
    }
}