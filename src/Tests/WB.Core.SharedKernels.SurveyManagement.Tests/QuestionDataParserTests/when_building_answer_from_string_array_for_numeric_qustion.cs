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
    internal class when_building_answer_from_string_array_for_numeric_qustion : QuestionDataParserTestContext
    {
        Establish context = () =>
        {
            answer = "1";
            questionDataParser = CreateQuestionDataParser(); 
        };

        Because of =
            () => result = questionDataParser.BuildAnswerFromStringArray(new string[]{answer}, questionVarName, CreateQuestionnaireDocumentWithOneChapter(new NumericQuestion() { PublicKey = questionId, QuestionType = QuestionType.Numeric, IsInteger = true, StataExportCaption = questionVarName }));

        It should_result_be_equal_to_1 = () =>
            result.Value.Value.ShouldEqual(1);

        It should_result_key_be_equal_to_questionId = () =>
            result.Value.Key.ShouldEqual(questionId);
    }
}
