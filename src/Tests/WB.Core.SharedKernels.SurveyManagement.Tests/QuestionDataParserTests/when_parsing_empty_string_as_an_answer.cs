using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.QuestionDataParserTests
{
    internal class when_parsing_empty_string_as_an_answer : QuestionDataParserTestContext
    {
        Establish context = () => { questionDataParser = CreateQuestionDataParser(); };

        Because of =
            () => parsingResult = questionDataParser.TryParse(string.Empty, "var", new QuestionnaireDocument(), out parcedValue);

        It should_result_be_ValueIsNullOrEmpty = () =>
            parsingResult.ShouldEqual(ValueParsingResult.ValueIsNullOrEmpty);
    }
}
