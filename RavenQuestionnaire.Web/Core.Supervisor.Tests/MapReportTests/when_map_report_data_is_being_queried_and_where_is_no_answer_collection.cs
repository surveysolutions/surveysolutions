using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Supervisor.Views.Interview;
using Core.Supervisor.Views.Reposts.Factories;
using Core.Supervisor.Views.Reposts.InputModels;
using Core.Supervisor.Views.Reposts.Views;
using Machine.Specifications;
using Moq;
using It = Machine.Specifications.It;

namespace Core.Supervisor.Tests.MapReportTests
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

        private It should_return_view_with_empty_collection_of_answers = () =>
            view.Answers.Length.ShouldEqual(0);


        private static MapReport mapReport;
        private static MapReportInputModel input;
        private static MapReportView view;
    }
}
