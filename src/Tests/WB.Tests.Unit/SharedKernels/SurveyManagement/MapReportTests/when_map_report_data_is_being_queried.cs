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
    internal class when_map_report_data_is_being_queried : MapReportTestContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("11111111111111111111111111111111");
            var questionnaireIdentity = "11111111111111111111111111111111$1";
            var variableName = "var";

            input = new MapReportInputModel {
                Variable = variableName,
                QuestionnaireId = questionnaireIdentity,
                NorthEastCornerLatitude = 80,
                NorthEastCornerLongtitude = 175,
                SouthWestCornerLatitude = -80,
                SouthWestCornerLongtitude = 160
            };

            var repositoryReader = new TestInMemoryWriter<MapReportPoint>();
            
            repositoryReader.Store(Create.Entity.MapReportPoint("id1", -75.11, -171.21, interview1Id, questionnaireId), "id1");
            repositoryReader.Store(Create.Entity.MapReportPoint("id2", 72.555, 170.32, interview2Id, questionnaireId), "id2");

            mapReport = CreateMapReport(repositoryReader);
        };

        Because of = () =>
            view = mapReport.Load(input);

        It should_return_view_with_2_records_in_collection_of_points = () =>
            view.Points.Length.ShouldEqual(1);

        It should_interview_id_in_second_point_be_equal_to_interview2Id = () =>
            view.Points[0].Id.ShouldEqual(interview2Id.ToString());

        It should_answers_in_second_point_be_specified_value = () =>
            view.Points[0].Answers.ShouldEqual("72.555;170.32");

        private static Guid interview1Id = Guid.Parse("11111111111111111111111111111111");
        private static Guid interview2Id = Guid.Parse("22222222222222222222222222222222");
        private static MapReport mapReport;
        private static MapReportInputModel input;
        private static MapReportView view;
    }
}