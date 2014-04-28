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
    internal class when_pasing_answer_on_geolocation_question_which_cant_be_parsed : QuestionDataParserTestContext
    {
        Establish context = () => { questionDataParser = CreateQuestionDataParser(); };

        Because of =
            () => result = questionDataParser.Parse(answer, questionVarName, CreateQuestionnaireDocumentWithOneChapter(new GpsCoordinateQuestion() { PublicKey = questionId, QuestionType = QuestionType.GpsCoordinates, StataExportCaption = questionVarName }));

        It should_result_be_null = () =>
            result.ShouldBeNull();

        private static QuestionDataParser questionDataParser;
        private static KeyValuePair<Guid, object>? result;
        private static Guid questionId = Guid.NewGuid();
        private static string questionVarName = "var";
        private static string answer = "unparsed";
    }
}
