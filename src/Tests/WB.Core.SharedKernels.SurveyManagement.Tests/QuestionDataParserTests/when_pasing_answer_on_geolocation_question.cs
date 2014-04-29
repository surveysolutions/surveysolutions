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
    internal class when_pasing_answer_on_geolocation_question : QuestionDataParserTestContext
    {
        Establish context = () =>
        {
            answer = "1,2[34]";
            questionDataParser = CreateQuestionDataParser(); };

        Because of =
            () => result = questionDataParser.Parse(answer, questionVarName, CreateQuestionnaireDocumentWithOneChapter(new GpsCoordinateQuestion() { PublicKey = questionId, QuestionType = QuestionType.GpsCoordinates, StataExportCaption = questionVarName }));

        It should_result_be_type_of_GeoPosition = () =>
            result.Value.Value.ShouldBeOfExactType<GeoPosition>();

        It should_result_key_be_equal_to_questionId = () =>
            result.Value.Key.ShouldEqual(questionId);
    }
}
