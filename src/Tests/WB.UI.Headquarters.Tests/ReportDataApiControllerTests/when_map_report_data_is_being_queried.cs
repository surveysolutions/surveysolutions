using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.View;
using Moq;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.UI.Headquarters.Controllers;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.UI.Headquarters.Tests.ReportDataApiControllerTests
{
    internal class when_map_report_data_is_being_queried : ReportDataApiControllerTestContext
    {
        Establish context = () =>
        {
            mapReportFactoryMock = new Mock<IViewFactory<MapReportInputModel, MapReportView>>();
            mapReportFactoryMock.Setup(x => x.Load(input)).Returns(view);
            controller = CreateReportDataApiController(mapReport: mapReportFactoryMock.Object);
        };

        Because of = () =>
             resultView = controller.MapReport(input);

        It should_load_data_from_factory_once = () =>
            mapReportFactoryMock.Verify(x => x.Load(input), Times.Once());

        It should_return_same_view_as_was_setted_up = () =>
            resultView.ShouldBeTheSameAs(view);

        private static MapReportInputModel input;
        private static MapReportView view;
        private static MapReportView resultView;
        private static ReportDataApiController controller;
        private static Mock<IViewFactory<MapReportInputModel, MapReportView>> mapReportFactoryMock;

    }
}
