using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.QuestionDataParserTests
{
    internal class when_pasing_answer_on_datetime_question : QuestionDataParserTestContext
    {
        Establish context = () =>
        {
            answer = "4/28/2014";
            questionDataParser = CreateQuestionDataParser(); 
        };

        Because of =
            () => result = questionDataParser.Parse(answer, questionVarName, CreateQuestionnaireDocumentWithOneChapter(new DateTimeQuestion() { PublicKey = questionId, QuestionType = QuestionType.DateTime, StataExportCaption = questionVarName }));

        It should_result_be_equal_to_4_28_2014 = () =>
            result.Value.Value.ShouldEqual(DateTime.Parse("4/28/2014", CultureInfo.InvariantCulture.DateTimeFormat));

        It should_result_key_be_equal_to_questionId = () =>
            result.Value.Key.ShouldEqual(questionId);
    }
}
