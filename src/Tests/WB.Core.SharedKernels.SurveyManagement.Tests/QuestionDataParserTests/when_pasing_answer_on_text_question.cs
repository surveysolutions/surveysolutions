using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.QuestionDataParserTests
{
    internal class when_pasing_answer_on_text_question : QuestionDataParserTestContext
    {
        Establish context = () =>
        {
            answer = "some answer";
            questionDataParser = CreateQuestionDataParser(); };

        Because of =
            () => result = questionDataParser.Parse(answer, questionVarName, CreateQuestionnaireDocumentWithOneChapter(new TextQuestion() { PublicKey = questionId, QuestionType = QuestionType.Text, StataExportCaption = questionVarName }));

        It should_result_value_be_equal_to_answer = () =>
            result.Value.Value.ShouldEqual(answer);

        It should_result_key_be_equal_to_questionId = () =>
            result.Value.Key.ShouldEqual(questionId);
    }
}
