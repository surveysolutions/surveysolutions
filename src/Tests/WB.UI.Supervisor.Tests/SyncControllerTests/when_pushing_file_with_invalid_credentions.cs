using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
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
            exception = Catch.Exception(() =>
                (JsonResult)controller.PostFile("login", "password", Guid.NewGuid())) as HttpException;

        It should_http_exception_be_rised = () =>
            exception.ShouldNotBeNull();

        It should_exception_http_code_be_equal_to_403 = () =>
            exception.GetHttpCode().ShouldEqual(403);

        private static SyncController controller;
        private static HttpException exception;
    }
}
