using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionDataParserTests
{
    internal class when_building_answer_from_string_array_for_gps_qustion : QuestionDataParserTestContext
    {
        Establish context = () =>
        {
            questionDataParser = CreateQuestionDataParser();
            question = Create.Entity.GpsCoordinateQuestion(questionId: questionId, variable: questionVarName);
        };

        Because of = () =>
            gpsResult = questionDataParser.BuildAnswerFromStringArray(new[] { new Tuple<string, string>(questionVarName + "__Latitude", "1"), new Tuple<string, string>(questionVarName + "__Longitude", "2"), new Tuple<string, string>(questionVarName + "__Accuracy", "3") }, question) as GpsAnswer;

        It should_return_result_of_type_GeoPosition = () =>
            gpsResult.ShouldNotBeNull();

        It should_return_result_with_Latitude_eqal_to_1 = () =>
            gpsResult.Value.Latitude.ShouldEqual(1);

        It should_return_result_with_Latitude_eqal_to_2 = () =>
           gpsResult.Value.Longitude.ShouldEqual(2);

        It should_return_result_with_Accuracy_eqal_to_3 = () =>
           gpsResult.Value.Accuracy.ShouldEqual(3);

        protected static GpsAnswer gpsResult;
    }
}