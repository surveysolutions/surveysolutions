using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Machine.Specifications;
using Main.Core.View;
using Main.Core.View.User;
using Moq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using It = Machine.Specifications.It;

namespace WB.UI.Supervisor.Tests.SyncControllerTests
{
    internal class when_pushing_file_with_invalid_credentions : SyncControllerTestContext
    {
        Establish context = () =>
        {
            controller = CreateSyncController();
        };

        Because of = () =>
            result = (JsonResult)controller.PostFile("login","password",Guid.NewGuid());

        It should_return_false_result = () =>
            ((bool) result.Data).ShouldEqual(false);

        private static SyncController controller;
        private static JsonResult result;
    }
}
