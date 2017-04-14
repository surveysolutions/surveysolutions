using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.MapReportTests
{
    internal class when_map_report_data_is_being_queried_and_where_is_answer_collection : MapReportTestContext
    {
        Establish context = () =>
        {
            var questionnaireIdentity = "11111111111111111111111111111111$1";
            var questionnaireId = Guid.Parse("11111111111111111111111111111111");
            var variableName = "var";

            input = new MapReportInputModel
            {
                Variable = variableName,
                QuestionnaireId = questionnaireIdentity,
                NorthEastCornerLatitude = 90,
                NorthEastCornerLongtitude = 180,
                SouthWestCornerLatitude = -90,
                SouthWestCornerLongtitude = -180
            };

            var repositoryReader = new TestInMemoryWriter<MapReportPoint>();

            repositoryReader.Store(Create.Entity.MapReportPoint("id1", 11.11, 11.11, interview1Id, questionnaireId), "id1");
            repositoryReader.Store(Create.Entity.MapReportPoint("id2", 22, 22, interview1Id, questionnaireId), "id2");
            repositoryReader.Store(Create.Entity.MapReportPoint("id3", 55.55, 66.666, interview2Id, questionnaireId), "id3");

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