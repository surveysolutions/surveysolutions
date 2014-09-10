using System;
using System.Web.Mvc;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Supervisor.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.UI.Supervisor.Controllers;
using It = Machine.Specifications.It;

namespace WB.UI.Supervisor.Tests.HQSyncControllerTests
{
    internal class when_pushing_data_to_hq : HQSyncControllerTestsContext
    {
        Establish context = () =>
        {
            var globalInfoProvider = Mock.Of<IGlobalInfoProvider>(provider
                => provider.GetCurrentUser().Id == userId);

            controller = Create.HQSyncController(synchronizer: synchronizerMock.Object, globalInfoProvider: globalInfoProvider);
        };

        Because of = () =>
            result = controller.Push();

        It should_push_data_via_synchronizer_using_user_id_from_global_info_provider = () =>
            synchronizerMock.Verify(synchronizer => synchronizer.Push(userId), Times.Once);

        It should_return_json_result = () =>
            result.ShouldBeOfExactType<JsonResult>();

        private static ActionResult result;
        private static HQSyncController controller;
        private static Mock<ISynchronizer> synchronizerMock = new Mock<ISynchronizer>();
        private static Guid userId = Guid.Parse("11111111111111111111111111111111");
    }
}