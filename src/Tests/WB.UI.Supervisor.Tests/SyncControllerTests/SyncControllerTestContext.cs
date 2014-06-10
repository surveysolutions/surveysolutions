using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Main.Core.View;
using Main.Core.View.User;
using Moq;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Synchronization;
using WB.UI.Supervisor.Controllers;

namespace WB.UI.Supervisor.Tests.SyncControllerTests
{
    internal class SyncControllerTestContext
    {
        protected static SyncController CreateSyncController(ISyncManager syncManager = null,
            ILogger logger = null,
            IViewFactory<UserViewInputModel, UserView> viewFactory = null,
            int supervisorVersion = 5)
        {
            var controller = new SyncController(syncManager ?? Mock.Of<ISyncManager>(),
                logger ?? Mock.Of<ILogger>(),
                viewFactory ?? Mock.Of<IViewFactory<UserViewInputModel, UserView>>(),
                (login, password) => true,
                (login, role) => true,
                (type) => supervisorVersion);

            SetControllerContextWithStream(controller, stream: null);
            
            return controller;
        }

        protected static void SetControllerContextWithStream(Controller controller, Stream stream)
        {
            var context = new Mock<HttpContextBase>();
            var request = new Mock<HttpRequestBase>();
            context.Setup(x => x.Request).Returns(request.Object);
            context.Setup(x => x.IsDebuggingEnabled).Returns(false);

            var headers = new NameValueCollection()
            {
                {"Authorization","Some value"}
            };
            request.Setup(x => x.Headers).Returns(headers);

            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);
        }
    }
}
