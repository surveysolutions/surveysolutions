using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Supervisor.Views.Reposts.InputModels;
using Core.Supervisor.Views.Reposts.Views;
using Machine.Specifications;
using Main.Core.View;
using Moq;
using Web.Supervisor.Controllers;
using It = Machine.Specifications.It;

namespace Web.Supervisor.Tests.ReportDataApiControllerTests
{
    internal class when_map_report_data_is_being_queried : ReportDataApiControllerTestContext
    {
        Establish context = () =>
        {
            mapReportFactoryMock = new Mock<IViewFactory<MapReportInputModel, MapReportView>>();
            controller = CreateReportDataApiController(mapReport: mapReportFactoryMock.Object);
        };

        Because of = () =>
            controller.MapReport(input);

        It should_load_data_from_factory_once = () =>
            mapReportFactoryMock.Verify(x => x.Load(input), Times.Once());

        private static MapReportInputModel input;
        private static ReportDataApiController controller;
        private static Mock<IViewFactory<MapReportInputModel, MapReportView>> mapReportFactoryMock;

    }
}
