using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.QuestionDataParserTests
{
    internal class when_pasing_answer_on_geolocation_question : QuestionDataParserTestContext
    {
        private Establish context = () =>
        {
            answer = "1,2[34]";
            questionDataParser = CreateQuestionDataParser();
        };

        private Because of =
            () =>
                parsingResult =
                    questionDataParser.TryParse(answer, new GpsCoordinateQuestion()
                    {
                        PublicKey = questionId,
                        QuestionType = QuestionType.GpsCoordinates,
                        StataExportCaption = questionVarName
                    }, out parcedValue);

        private It should_result_be_type_of_GeoPosition = () =>
            parcedValue.Value.ShouldBeOfExactType<GeoPosition>();

        private It should_result_key_be_equal_to_questionId = () =>
            parcedValue.Key.ShouldEqual(questionId);
    }
}