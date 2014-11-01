using System.Collections.Specialized;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using Main.Core.View;
using Main.Core.View.User;
using Moq;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Web.Api;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.Core.Synchronization;

namespace WB.UI.Supervisor.Tests.SyncControllerTests
{
    internal class SyncControllerTestContext
    {
        protected static InterviewerSyncController CreateSyncController(
            ICommandService commandService = null,
            IGlobalInfoProvider globalInfo = null,
            ISyncManager syncManager = null,
            ILogger logger = null, IViewFactory<UserViewInputModel, UserView> viewFactory = null,
            ISupportedVersionProvider versionProvider = null)
        {
            var controller = CreateSyncControllerImpl(commandService, globalInfo, syncManager, logger, viewFactory, versionProvider);
            SetControllerContextWithStream(controller, stream: null);
            
            return controller;
        }

        protected static InterviewerSyncController CreateSyncControllerWithFile(
            ICommandService commandService = null,
            IGlobalInfoProvider globalInfo = null,
            ISyncManager syncManager = null,
            ILogger logger = null,
            IViewFactory<UserViewInputModel, UserView> viewFactory = null,
            ISupportedVersionProvider versionProvider = null, IPlainInterviewFileStorage plainFileRepository = null, 
            Stream stream = null, string fileName = null)
        {
            var controller = CreateSyncControllerImpl(commandService, globalInfo, syncManager, logger, viewFactory, versionProvider, plainFileRepository);
            SetControllerContextWithFiles(controller, stream: stream, fileName: fileName);

            return controller;
        }

        private static InterviewerSyncController CreateSyncControllerImpl(
            ICommandService commandService = null,
            IGlobalInfoProvider globalInfo = null,
            ISyncManager syncManager = null,
            ILogger logger = null,
            IViewFactory<UserViewInputModel, UserView> viewFactory = null,
            ISupportedVersionProvider versionProvider = null, IPlainInterviewFileStorage plainFileRepository = null)
        {
            var controller = new InterviewerSyncController(
                commandService ?? Mock.Of<ICommandService>(), 
                globalInfo ?? Mock.Of<IGlobalInfoProvider>(),
                syncManager ?? Mock.Of<ISyncManager>(),
                logger ?? Mock.Of<ILogger>(),
                viewFactory ?? Mock.Of<IViewFactory<UserViewInputModel, UserView>>(),
                versionProvider ?? Mock.Of<ISupportedVersionProvider>(), 
                (login, role) => true, 
                plainFileRepository ?? Mock.Of<IPlainInterviewFileStorage>());

            return controller;
        }

        protected static void SetControllerContextWithStream(ApiController controller, Stream stream)
        {
            controller.Request = new HttpRequestMessage(HttpMethod.Post, "http://localhost");
            controller.Configuration = new System.Web.Http.HttpConfiguration(new System.Web.Http.HttpRouteCollection());
        }

        protected static void SetControllerContextWithFiles(ApiController controller, Stream stream, string fileName = null)
        {
            
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "http://localhost");
            var content = new MultipartFormDataContent();

            content.Add(new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes("This is from a file"))), "Data", fileName ?? "fileName");
            
            requestMessage.Content = content;
            controller.Request = requestMessage; 
            controller.Configuration = new System.Web.Http.HttpConfiguration(new System.Web.Http.HttpRouteCollection());
        }
    }
}
