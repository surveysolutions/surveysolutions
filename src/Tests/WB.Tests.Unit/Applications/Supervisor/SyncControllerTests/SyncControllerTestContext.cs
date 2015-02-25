using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Moq;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Web.Api;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.Core.Synchronization;

namespace WB.Tests.Unit.Applications.Supervisor.SyncControllerTests
{
    internal class SyncControllerTestContext
    {
        protected static InterviewerSyncController CreateSyncController(
            ICommandService commandService = null,
            IGlobalInfoProvider globalInfo = null,
            ISyncManager syncManager = null,
            ILogger logger = null, 
            IUserWebViewFactory viewFactory = null,
            ISupportedVersionProvider versionProvider = null,
            ISyncProtocolVersionProvider syncVersionProvider = null,
            IJsonUtils jsonUtils = null)
        {
            var controller = CreateSyncControllerImpl(commandService, globalInfo, syncManager, logger, viewFactory, versionProvider, syncVersionProvider,
                jsonUtils: jsonUtils);
            SetControllerContextWithStream(controller, stream: null);
            
            return controller;
        }

        protected static InterviewerSyncController CreateSyncControllerWithFile(
            ICommandService commandService = null,
            IGlobalInfoProvider globalInfo = null,
            ISyncManager syncManager = null,
            ILogger logger = null,
            IUserWebViewFactory viewFactory = null,
            ISupportedVersionProvider versionProvider = null,
            ISyncProtocolVersionProvider syncVersionProvider = null,
            IPlainInterviewFileStorage plainFileRepository = null, 
            Stream stream = null, string fileName = null,
            IFileSystemAccessor fileSystemAccessor = null)
        {
            var controller = CreateSyncControllerImpl(commandService, globalInfo, syncManager, logger, viewFactory, versionProvider, syncVersionProvider, plainFileRepository, fileSystemAccessor);
            SetControllerContextWithFiles(controller, stream: stream, fileName: fileName);

            return controller;
        }

        private static InterviewerSyncController CreateSyncControllerImpl(
            ICommandService commandService = null,
            IGlobalInfoProvider globalInfo = null,
            ISyncManager syncManager = null,
            ILogger logger = null,
            IUserWebViewFactory viewFactory = null,
            ISupportedVersionProvider versionProvider = null,
            ISyncProtocolVersionProvider syncVersionProvider = null,
            IPlainInterviewFileStorage plainFileRepository = null,
            IFileSystemAccessor fileSystemAccessor = null,
            ITabletInformationService tabletInformationService = null,
            IJsonUtils jsonUtils = null)
        {
            var controller = new InterviewerSyncController(
                commandService ?? Mock.Of<ICommandService>(), 
                globalInfo ?? Mock.Of<IGlobalInfoProvider>(),
                syncManager ?? Mock.Of<ISyncManager>(),
                logger ?? Mock.Of<ILogger>(),
                viewFactory ?? Mock.Of<IUserWebViewFactory>(),
                versionProvider ?? Mock.Of<ISupportedVersionProvider>(),
                plainFileRepository ?? Mock.Of<IPlainInterviewFileStorage>(),
                fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>(),
                syncVersionProvider ?? Mock.Of<ISyncProtocolVersionProvider>(),
                tabletInformationService ?? Mock.Of<ITabletInformationService>(),
                jsonUtils ?? Mock.Of<IJsonUtils>());

            return controller;
        }

        protected static void SetControllerContextWithStream(ApiController controller, Stream stream)
        {
            controller.Request = new HttpRequestMessage(HttpMethod.Post, "http://localhost");
            controller.Configuration = new HttpConfiguration(new HttpRouteCollection());
        }

        protected static void SetControllerContextWithFiles(ApiController controller, Stream stream, string fileName = null)
        {
            
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "http://localhost");
            var content = new MultipartFormDataContent();

            content.Add(new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes("This is from a file"))), "Data", fileName ?? "fileName");
            
            requestMessage.Content = content;
            controller.Request = requestMessage; 
            controller.Configuration = new HttpConfiguration(new HttpRouteCollection());
        }

        protected static SyncPackageRequest CreateSyncPackageRequest(string packageId, Guid deviceId)
        {
            return new SyncPackageRequest
            {
                ClientRegistrationId = deviceId,
                PackageId = packageId
            };
        }

        protected static UserSyncPackageDto CreateUserSyncPackageDto(Guid userId, string packageId)
        {
            return new UserSyncPackageDto
            {
                PackageId = packageId
            };
        }

        protected static QuestionnaireSyncPackageDto CreateQuestionnaireSyncPackageDto(string packageId)
        {
            return new QuestionnaireSyncPackageDto
            {
                PackageId = packageId
            };
        }

        protected static InterviewSyncPackageDto CreateInterviewSyncPackageDto(string packageId)
        {
            return new InterviewSyncPackageDto
            {
                PackageId = packageId
            };
        }

        protected static SyncItemsMetaContainer CreateSyncItemsMetaContainer()
        {
            return new SyncItemsMetaContainer
                   {
                       SyncPackagesMeta = new List<SynchronizationChunkMeta>()
                   };
        }

        protected static SyncItemsMetaContainerRequest CreateSyncItemsMetaContainerRequest(string lastPackageId, Guid deviceId)
        {
            return new SyncItemsMetaContainerRequest
                   {
                       ClientRegistrationId = deviceId,
                       LastSyncedPackageId = lastPackageId
                   };
        }
    }
}
