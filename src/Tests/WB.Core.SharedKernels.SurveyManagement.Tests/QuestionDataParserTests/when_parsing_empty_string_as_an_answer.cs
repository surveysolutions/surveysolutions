﻿using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.QuestionDataParserTests
{
    internal class when_parsing_empty_string_as_an_answer : QuestionDataParserTestContext
    {
        private Establish context = () => { questionDataParser = CreateQuestionDataParser(); };

        private Because of =
            () => parsingResult = questionDataParser.TryParse(string.Empty, null, new QuestionnaireDocument(), out parcedValue);

        private It should_result_be_ValueIsNullOrEmpty = () =>
            parsingResult.ShouldEqual(ValueParsingResult.ValueIsNullOrEmpty);
    }
}