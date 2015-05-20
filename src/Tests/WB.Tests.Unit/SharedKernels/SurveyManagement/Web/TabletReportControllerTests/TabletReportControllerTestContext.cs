using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.TabletInformation;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.TabletReportControllerTests
{
    internal class TabletReportControllerTestContext
    {
        protected static TabletReportController CreateTabletReportController(ITabletInformationService tabletInformationService = null)
        {
            return new TabletReportController(Mock.Of<ILogger>(),
                tabletInformationService ??
                    Mock.Of<ITabletInformationService>(
                        _ =>
                            _.GetFullPathToContentFile(It.IsAny<string>()) == "result" &&
                                _.GetAllTabletInformationPackages() == new List<TabletInformationView>()));
        }

        protected static void SetControllerContextWithStream(Controller controller, Stream stream)
        {
            var context = new Mock<HttpContextBase>();
            var request = new Mock<HttpRequestBase>();
            context.Setup(x => x.Request).Returns(request.Object);
            
            request.Setup(x => x.InputStream).Returns(stream);
            
            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);
        }

        protected static Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
