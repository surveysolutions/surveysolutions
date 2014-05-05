﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Machine.Specifications;
using Moq;
using WB.UI.Supervisor.Controllers;
using It = Machine.Specifications.It;

namespace WB.UI.Supervisor.Tests.TabletReportControllerTests
{
    internal class when_posting_info_package_is_caller_with_broken_content : TabletReportControllerTestContext
    {
        Establish context = () =>
        {
            tabletReportController = CreateTabletReportController();
            SetControllerContextWithStream(tabletReportController, GenerateStreamFromString("random string"));
        };

        Because of = () => result = tabletReportController.PostInfoPackage() as JsonResult;

        It should_return_JsonResult = () => result.ShouldNotBeNull();

        It should_return_false = () => result.Data.ShouldEqual(false);

        private static TabletReportController tabletReportController;
        private static JsonResult result;
    }
}
