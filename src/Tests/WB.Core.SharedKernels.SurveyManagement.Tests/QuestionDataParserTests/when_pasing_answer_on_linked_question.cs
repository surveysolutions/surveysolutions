using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.QuestionDataParserTests
{
    internal class when_pasing_answer_on_linked_question : QuestionDataParserTestContext
    {
        Establish context = () => { questionDataParser = CreateQuestionDataParser(); };

        Because of =
            () => result = questionDataParser.Parse("some answer", questionVarName, CreateQuestionnaireDocumentWithOneChapter(new SingleQuestion() { LinkedToQuestionId = Guid.NewGuid(), StataExportCaption = questionVarName }));

        It should_result_be_null = () =>
            result.ShouldBeNull();
    }
}
