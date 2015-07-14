using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionDataParserTests
{
    internal class when_building_answer_from_string_array_for_gps_qustion : QuestionDataParserTestContext
    {
        Establish context = () =>
        {
            questionDataParser = CreateQuestionDataParser();
            question = Create.GpsCoordinateQuestion(questionId: questionId, variableName: questionVarName);
        };

        Because of =
            () =>
                gpsResult =
                    questionDataParser.BuildAnswerFromStringArray(new[] { new Tuple<string, string>(questionVarName + "_Latitude", "1"), new Tuple<string, string>(questionVarName + "_Longitude", "2"), new Tuple<string, string>(questionVarName + "_Accuracy", "3") },
                        question, CreateQuestionnaireDocumentWithOneChapter(question)).Value.Value as GeoPosition;

        It should_return_result_of_type_GeoPosition = () =>
            gpsResult.ShouldNotBeNull();

        It should_return_result_with_Latitude_eqal_to_1 = () =>
            gpsResult.Latitude.ShouldEqual(1);

        It should_return_result_with_Latitude_eqal_to_2 = () =>
           gpsResult.Longitude.ShouldEqual(2);

        It should_return_result_with_Accuracy_eqal_to_3 = () =>
           gpsResult.Accuracy.ShouldEqual(3);

        protected static GeoPosition gpsResult;
    }
}