using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Net.Http.Headers;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.Implementation;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Versions;
using WB.Core.SharedKernels.DataCollection;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;
using WB.UI.Headquarters.API;
using WB.UI.Headquarters.Controllers.Api.DataCollection.Supervisor.v1;
using WB.UI.Headquarters.Services;

namespace WB.Tests.Web.Headquarters.Controllers.SupervisorApiTests
{
    [TestOf(nameof(SupervisorControllerBase.CheckCompatibility))]
    public class SupervisorCheckCompatibilityTests
    {
        private const string SupervisorUserAgent = "org.worldbank.solutions.supervisor/{0} (QuestionnaireVersion/27.0.0)";

        [Test]
        public async Task when_supervisor_apk_is_not_stored_should_return_null_for_latest_version()
        {
            const int currentServerBuildNumber = 38141;

            var clientApkProvider = new Mock<IClientApkProvider>();
            clientApkProvider.Setup(x => x.GetApplicationBuildNumber(ClientApkInfo.SupervisorFileName))
                .ReturnsAsync((int?)null);

            var controller = new SupervisorControllerBase(
                Mock.Of<ITabletInformationService>(),
                new SupervisorSyncProtocolVersionProvider(),
                Mock.Of<IUserViewFactory>(),
                Mock.Of<IPlainKeyValueStorage<InterviewerSettings>>(),
                new TestPlainStorage<ServerSettings>(),
                clientApkProvider.Object,
                Mock.Of<IAuthorizedUser>(),
                Mock.Of<IInterviewInformationFactory>(),
                Mock.Of<IInterviewerVersionReader>(),
                Mock.Of<IProductVersion>(x => x.GetBuildNumber() == currentServerBuildNumber));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var latestVersion = await controller.GetLatestVersion();

            Assert.That(latestVersion, Is.Null);
        }

        [Test]
        public async Task when_latest_version_is_requested_for_compatibility_and_apk_is_not_stored_should_return_current_server_build()
        {
            const int currentServerBuildNumber = 38141;
            var clientApkProvider = new Mock<IClientApkProvider>();
            clientApkProvider.Setup(x => x.GetApplicationBuildNumber(ClientApkInfo.SupervisorFileName))
                .ReturnsAsync((int?)null);

            var controller = new SupervisorControllerBase(
                Mock.Of<ITabletInformationService>(),
                new SupervisorSyncProtocolVersionProvider(),
                Mock.Of<IUserViewFactory>(),
                Mock.Of<IPlainKeyValueStorage<InterviewerSettings>>(),
                new TestPlainStorage<ServerSettings>(),
                clientApkProvider.Object,
                Mock.Of<IAuthorizedUser>(),
                Mock.Of<IInterviewInformationFactory>(),
                Mock.Of<IInterviewerVersionReader>(),
                Mock.Of<IProductVersion>(x => x.GetBuildNumber() == currentServerBuildNumber));

            var latestVersion = await controller.GetLatestVersion(forCompatibilityCheck: true);

            Assert.That(latestVersion, Is.EqualTo(currentServerBuildNumber));
        }

        [Test]
        public async Task when_apk_not_stored_on_server_and_auto_update_disabled_should_not_return_406()
        {
            const int clientBuildNumber = 38141;
            var supervisorUserAgent = string.Format(SupervisorUserAgent, $"25.06.0 (build {clientBuildNumber})");

            var interviewerVersionReader = new Mock<IInterviewerVersionReader>();
            interviewerVersionReader.Setup(x => x.SupervisorBuildNumber())
                .ReturnsAsync((int?)null);

            var productVersion = Mock.Of<IProductVersion>();

            var interviewerSettings = Abc.Create.Entity.InterviewerSettings(autoUpdateEnabled: false);
            var interviewerSettingsStorage = Mock.Of<IPlainKeyValueStorage<InterviewerSettings>>(m =>
                m.GetById(AppSetting.InterviewerSettings) == interviewerSettings);

            var controller = Web.Create.Controller.SupervisorApiController(
                interviewerVersionReader: interviewerVersionReader.Object,
                productVersion: productVersion,
                interviewerSettings: interviewerSettingsStorage);

            controller.Request.Headers[HeaderNames.UserAgent] = supervisorUserAgent;

            // Act
            IActionResult result = await controller.CheckCompatibility("device", SupervisorSyncProtocolVersionProvider.V4_MultiWorkspacesIntroduced);

            // Assert
            Assert.That(((IStatusCodeActionResult)result).StatusCode, Is.Not.EqualTo(StatusCodes.Status406NotAcceptable));
        }

        [Test]
        public async Task when_apk_not_stored_on_server_and_auto_update_enabled_should_return_426()
        {
            const int clientBuildNumber = 38141;
            var supervisorUserAgent = string.Format(SupervisorUserAgent, $"25.06.0 (build {clientBuildNumber})");

            var interviewerVersionReader = new Mock<IInterviewerVersionReader>();
            interviewerVersionReader.Setup(x => x.SupervisorBuildNumber())
                .ReturnsAsync((int?)null);

            var productVersion = Mock.Of<IProductVersion>();

            var interviewerSettings = Abc.Create.Entity.InterviewerSettings(autoUpdateEnabled: true);
            var interviewerSettingsStorage = Mock.Of<IPlainKeyValueStorage<InterviewerSettings>>(m =>
                m.GetById(AppSetting.InterviewerSettings) == interviewerSettings);

            var controller = Web.Create.Controller.SupervisorApiController(
                interviewerVersionReader: interviewerVersionReader.Object,
                productVersion: productVersion,
                interviewerSettings: interviewerSettingsStorage);

            controller.Request.Headers[HeaderNames.UserAgent] = supervisorUserAgent;

            // Act
            IActionResult result = await controller.CheckCompatibility("device", SupervisorSyncProtocolVersionProvider.V4_MultiWorkspacesIntroduced);

            // Assert
            Assert.That(((IStatusCodeActionResult)result).StatusCode, Is.EqualTo(StatusCodes.Status426UpgradeRequired));
        }

        [Test]
        public async Task when_apk_stored_on_server_and_client_is_newer_and_autoupdate_disabled_should_return_406()
        {
            const int serverApkBuildNumber = 35000;
            const int clientBuildNumber = 38141;
            var supervisorUserAgent = string.Format(SupervisorUserAgent, $"25.06.0 (build {clientBuildNumber})");

            var interviewerVersionReader = new Mock<IInterviewerVersionReader>();
            interviewerVersionReader.Setup(x => x.SupervisorBuildNumber())
                .ReturnsAsync((int?)serverApkBuildNumber);

            var productVersion = Mock.Of<IProductVersion>(x => x.GetBuildNumber() == serverApkBuildNumber);

            var interviewerSettings = Abc.Create.Entity.InterviewerSettings(autoUpdateEnabled: false);
            var interviewerSettingsStorage = Mock.Of<IPlainKeyValueStorage<InterviewerSettings>>(m =>
                m.GetById(AppSetting.InterviewerSettings) == interviewerSettings);

            var controller = Web.Create.Controller.SupervisorApiController(
                interviewerVersionReader: interviewerVersionReader.Object,
                productVersion: productVersion,
                interviewerSettings: interviewerSettingsStorage);

            controller.Request.Headers[HeaderNames.UserAgent] = supervisorUserAgent;

            // Act
            IActionResult result = await controller.CheckCompatibility("device", SupervisorSyncProtocolVersionProvider.V4_MultiWorkspacesIntroduced);

            // Assert
            Assert.That(((IStatusCodeActionResult)result).StatusCode, Is.EqualTo(StatusCodes.Status406NotAcceptable));
        }
    }
}
