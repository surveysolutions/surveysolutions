using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.QuestionDataParserTests
{
    internal class when_pasing_answer_on_linked_question : QuestionDataParserTestContext
    {
        private Establish context = () => { questionDataParser = CreateQuestionDataParser(); };

        private Because of =
            () =>
                parsingResult =
                    questionDataParser.TryParse("some answer", new SingleQuestion()
                    {
                        LinkedToQuestionId = Guid.NewGuid(),
                        StataExportCaption = questionVarName
                    }, out parcedValue);

        private It should_result_be_UnsupportedLinkedQuestion = () =>
            parsingResult.ShouldEqual(ValueParsingResult.UnsupportedLinkedQuestion);
    }
}