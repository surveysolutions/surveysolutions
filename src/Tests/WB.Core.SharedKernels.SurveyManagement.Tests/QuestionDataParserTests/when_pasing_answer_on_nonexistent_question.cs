using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.QuestionDataParserTests
{
    internal class when_pasing_answer_on_nonexistent_question : QuestionDataParserTestContext
    {
        private Establish context = () => { questionDataParser = CreateQuestionDataParser(); };

        private Because of =
            () => parsingResult = questionDataParser.TryParse("some answer", null, new QuestionnaireDocument(), out parcedValue);

        private It should_result_be_QuestionWasNotFound = () =>
            parsingResult.ShouldEqual(ValueParsingResult.QuestionWasNotFound);
    }
}