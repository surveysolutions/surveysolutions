using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.SurveyManagement.Services.HealthCheck;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.HealthCheck;
using WB.Core.SharedKernels.SurveyManagement.Web.Api;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.ApiTests
{
    internal class when_healthcheck_controller_getstatus : ApiTestContext
    {
        private Establish context = () =>
        {
            checkResults = new HealthCheckResults(HealthCheckStatus.Warning, null, null, null, null, null);
            serviceMock = new Mock<IHealthCheckService>();
            serviceMock.Setup(m => m.Check()).Returns(checkResults);

            controller = CreateHealthCheckApiController(serviceMock.Object);
        };

        Because of = () =>
        {
            result = controller.GetStatus();
        };

        It should_return_HealthCheckStatus = () =>
            result.ShouldBeOfExactType<HealthCheckStatus>();

        It should_return_Happy_status = () =>
            result.ShouldEqual(checkResults.Status);

        It should_call_IHealthCheckService_Check_once = () =>
            serviceMock.Verify(x => x.Check(), Times.Once());


        private static HealthCheckResults checkResults;
        private static HealthCheckStatus result;
        private static Mock<IHealthCheckService> serviceMock;
        private static HealthCheckApiController controller;

    }
}
