using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionDataParserTests
{
    internal class when_pasing_answer_on_geolocation_question : QuestionDataParserTestContext
    {
        private Establish context = () =>
        {
            answer = "1";
            questionDataParser = CreateQuestionDataParser();
            question = new GpsCoordinateQuestion()
            {
                PublicKey = questionId,
                QuestionType = QuestionType.GpsCoordinates,
                StataExportCaption = questionVarName
            };
        };

        private Because of =
            () =>
                parsingResult =
                    questionDataParser.TryParse(answer, questionVarName + "__Latitude", question, out parcedValue, out parsedSingleColumnAnswer);

        private It should_result_be_type_of_double = () =>
            parcedValue.ShouldBeOfExactType<double>();
    }
}