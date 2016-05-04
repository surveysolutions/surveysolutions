using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.MapReportTests
{
    internal class when_map_report_data_is_being_queried_and_where_is_answer_collection : MapReportTestContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("11111111111111111111111111111111");
            long questionnaireVersion = 1;
            var variableName = "var";

            input = Mock.Of<MapReportInputModel>(x
                => x.Variable == variableName
                   && x.QuestionnaireId == questionnaireId
                   && x.QuestionnaireVersion == questionnaireVersion &&
                   x.NorthEastCornerLatitude == 90 &&
                   x.NorthEastCornerLongtitude == 180 &&
                   x.SouthWestCornerLatitude == -90 &&
                   x.SouthWestCornerLongtitude == -180);

            List<MapReportPoint> points = new List<MapReportPoint>();
            points.Add(new MapReportPoint("id1")
            {
                Latitude = 11.11,
                Longitude = 11.11,
                InterviewId = interview1Id,
                QuestionnaireId = questionnaireId,
                QuestionnaireVersion = questionnaireVersion,
                Variable = variableName
            });
            points.Add(new MapReportPoint("id2")
            {
                Latitude = 22,
                Longitude = 22,
                InterviewId = interview1Id,
                QuestionnaireId = questionnaireId,
                QuestionnaireVersion = questionnaireVersion,
                Variable = variableName
            });
            points.Add(new MapReportPoint("id3")
            {
                Latitude = 55.55,
                Longitude = 66.666,
                InterviewId = interview2Id,
                QuestionnaireId = questionnaireId,
                QuestionnaireVersion = questionnaireVersion,
                Variable = variableName
            });

            var repositoryReader = new TestInMemoryWriter<MapReportPoint>();
            foreach (var mapReportPoint in points)
            {
                repositoryReader.Store(mapReportPoint, mapReportPoint.Id);
            }

            mapReport = CreateMapReport(repositoryReader);
        };

        Because of = () =>
            view = mapReport.Load(input);

        It should_return_view_with_2_records_in_collection_of_points = () =>
            view.Points.Length.ShouldEqual(2);

        It should_interview_id_in_first_point_be_equal_to_interview1Id = () =>
            view.Points[0].Id.ShouldEqual(interview1Id.ToString());

        It should_answers_in_first_point_be_specified_value = () =>
            view.Points[0].Answers.ShouldEqual("11.11;11.11|22;22");

        It should_interview_id_in_second_point_be_equal_to_interview2Id = () =>
            view.Points[1].Id.ShouldEqual(interview2Id.ToString());

        It should_answers_in_second_point_be_specified_value = () =>
            view.Points[1].Answers.ShouldEqual("55.55;66.666");

        private static Guid interview1Id = Guid.Parse("11111111111111111111111111111111");
        private static Guid interview2Id = Guid.Parse("22222222222222222222222222222222");
        private static MapReport mapReport;
        private static MapReportInputModel input;
        private static MapReportView view;
    }
}