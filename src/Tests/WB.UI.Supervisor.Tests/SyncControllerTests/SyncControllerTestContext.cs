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
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.Synchronization;
using WB.UI.Supervisor.Controllers;

namespace WB.UI.Supervisor.Tests.SyncControllerTests
{
    internal class SyncControllerTestContext
    {
        protected static SyncController CreateSyncController(ISyncManager syncManager = null,
            ILogger logger = null,
            IViewFactory<UserViewInputModel, UserView> viewFactory = null,
            ISupportedVersionProvider versionProvider = null)
        {
            var controller = CreateSyncControllerImpl(syncManager, logger, viewFactory, versionProvider);
            SetControllerContextWithStream(controller, stream: null);
            
            return controller;
        }

        protected static SyncController CreateSyncControllerWithFile(ISyncManager syncManager = null,
            ILogger logger = null,
            IViewFactory<UserViewInputModel, UserView> viewFactory = null,
            ISupportedVersionProvider versionProvider = null, IPlainInterviewFileStorage plainFileRepository = null, Stream stream = null, string fileName = null)
        {
            var controller = CreateSyncControllerImpl(syncManager, logger, viewFactory, versionProvider, plainFileRepository);
            SetControllerContextWithFiles(controller, stream: stream, fileName: fileName);

            return controller;
        }

        private static SyncController CreateSyncControllerImpl(ISyncManager syncManager = null,
            ILogger logger = null,
            IViewFactory<UserViewInputModel, UserView> viewFactory = null,
            ISupportedVersionProvider versionProvider = null, IPlainInterviewFileStorage plainFileRepository = null)
        {
            var controller = new SyncController(syncManager ?? Mock.Of<ISyncManager>(),
                logger ?? Mock.Of<ILogger>(),
                viewFactory ?? Mock.Of<IViewFactory<UserViewInputModel, UserView>>(),
                versionProvider ?? Mock.Of<ISupportedVersionProvider>(), (login, password) => true, (login, role) => true, plainFileRepository ?? Mock.Of<IPlainInterviewFileStorage>());

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

        protected static void SetControllerContextWithFiles(Controller controller, Stream stream, string fileName = null)
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

            request.Setup(x => x.Files)
                .Returns(
                    Mock.Of<HttpFileCollectionBase>(
                        _ =>
                            _.Count == 1 &&
                                _[0] ==
                                    Mock.Of<HttpPostedFileBase>(
                                        file => file.FileName == (fileName??"fileName") && file.InputStream == stream)));

            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);
        }
    }
}
