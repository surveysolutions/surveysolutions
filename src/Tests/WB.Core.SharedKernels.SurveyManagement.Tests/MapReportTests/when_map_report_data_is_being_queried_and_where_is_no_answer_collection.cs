using System;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.MapReportTests
{
    internal class when_map_report_data_is_being_queried_and_where_is_no_answer_collection  : MapReportTestContext
    {
        Establish context = () =>
        {
            input = Mock.Of<MapReportInputModel>(x 
                => x.Variable == "var"
                && x.QuestionnaireId == Guid.Parse("44444444444444444444444444444444")
                && x.QuestionnaireVersion == 1);

            mapReport = CreateMapReport();
        };

        Because of = () => 
            view = mapReport.Load(input);

        It should_return_view_with_empty_collection_of_point = () =>
            view.Points.Length.ShouldEqual(0);


        private static MapReport mapReport;
        private static MapReportInputModel input;
        private static MapReportView view;
    }
}
