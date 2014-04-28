using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.QuestionDataParserTests
{
    internal class when_pasing_answer_on_nonexistent_question : QuestionDataParserTestContext
    {
        Establish context = () => { questionDataParser = CreateQuestionDataParser(); };

        Because of =
            () => result = questionDataParser.Parse("some answer", "var", new QuestionnaireDocument());

        It should_result_be_null = () =>
            result.ShouldBeNull();

        private static QuestionDataParser questionDataParser;
        private static KeyValuePair<Guid, object>? result;
    }
}
