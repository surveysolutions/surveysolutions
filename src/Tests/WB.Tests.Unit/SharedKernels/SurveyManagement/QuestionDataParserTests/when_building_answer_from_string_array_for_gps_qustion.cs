using System;
using FluentAssertions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionDataParserTests
{
    internal class when_building_answer_from_string_array_for_gps_qustion : QuestionDataParserTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionDataParser = CreateQuestionDataParser();
            question = Create.Entity.GpsCoordinateQuestion(questionId: questionId, variable: questionVarName);
            BecauseOf();
        }

        public void BecauseOf() =>
            gpsResult = questionDataParser.BuildAnswerFromStringArray(new[] { new Tuple<string, string>(questionVarName + "__Latitude", "1"), new Tuple<string, string>(questionVarName + "__Longitude", "2"), new Tuple<string, string>(questionVarName + "__Accuracy", "3") }, question) as GpsAnswer;

        [NUnit.Framework.Test] public void should_return_result_of_type_GeoPosition () =>
            gpsResult.Should().NotBeNull();

        [NUnit.Framework.Test] public void should_return_result_with_Latitude_eqal_to_1 () =>
            gpsResult.Value.Latitude.Should().Be(1);

        [NUnit.Framework.Test] public void should_return_result_with_Latitude_eqal_to_2 () =>
           gpsResult.Value.Longitude.Should().Be(2);

        [NUnit.Framework.Test] public void should_return_result_with_Accuracy_eqal_to_3 () =>
           gpsResult.Value.Accuracy.Should().Be(3);

        protected static GpsAnswer gpsResult;
    }
}
