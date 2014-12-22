using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;
using WB.Core.SharedKernels.SurveyManagement.Web.Api;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.ReportDataApiControllerTests
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

        private static MapReportInputModel input = new MapReportInputModel();
        private static MapReportView view = new MapReportView();
        private static MapReportView resultView;
        private static ReportDataApiController controller;
        private static Mock<IViewFactory<MapReportInputModel, MapReportView>> mapReportFactoryMock;

    }
}
