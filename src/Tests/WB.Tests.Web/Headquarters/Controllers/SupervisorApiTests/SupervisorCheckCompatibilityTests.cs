using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Net.Http.Headers;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.Implementation;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Versions;
using WB.Core.SharedKernels.DataCollection;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;
using WB.UI.Headquarters.Controllers.Api.DataCollection.Supervisor.v1;

namespace WB.Tests.Web.Headquarters.Controllers.SupervisorApiTests
{
    [TestOf(nameof(SupervisorControllerBase.CheckCompatibility))]
    public class SupervisorCheckCompatibilityTests
    {
        private const string SupervisorUserAgent = "org.worldbank.solutions.supervisor/{0} (QuestionnaireVersion/27.0.0)";

        [Test]
        public async Task when_apk_not_stored_on_server_and_client_is_newer_than_server_build_should_return_406()
        {
            const int serverHqBuildNumber = 35000;
            const int clientBuildNumber = 38141;
            var supervisorUserAgent = string.Format(SupervisorUserAgent, $"25.06.0 (build {clientBuildNumber})");

            var interviewerVersionReader = new Mock<IInterviewerVersionReader>();
            interviewerVersionReader.Setup(x => x.SupervisorBuildNumber())
                .ReturnsAsync((int?)null);

            var productVersion = Mock.Of<IProductVersion>(x => x.GetBuildNumber() == serverHqBuildNumber);

            // Auto-update disabled — client with recent build that exceeds server HQ version should be rejected
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

        [Test]
        public async Task when_apk_not_stored_on_server_and_auto_update_enabled_and_client_is_newer_than_server_build_should_return_406()
        {
            const int serverHqBuildNumber = 35000;
            const int clientBuildNumber = 38141;
            var supervisorUserAgent = string.Format(SupervisorUserAgent, $"25.06.0 (build {clientBuildNumber})");

            var interviewerVersionReader = new Mock<IInterviewerVersionReader>();
            interviewerVersionReader.Setup(x => x.SupervisorBuildNumber())
                .ReturnsAsync((int?)null);

            var productVersion = Mock.Of<IProductVersion>(x => x.GetBuildNumber() == serverHqBuildNumber);

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
            Assert.That(((IStatusCodeActionResult)result).StatusCode, Is.EqualTo(StatusCodes.Status406NotAcceptable));
        }

        [Test]
        public async Task when_apk_not_stored_on_server_and_client_matches_server_build_should_allow_sync()
        {
            const int serverHqBuildNumber = 35000;
            var supervisorUserAgent = string.Format(SupervisorUserAgent, $"25.06.0 (build {serverHqBuildNumber})");

            var interviewerVersionReader = new Mock<IInterviewerVersionReader>();
            interviewerVersionReader.Setup(x => x.SupervisorBuildNumber())
                .ReturnsAsync((int?)null);

            var productVersion = Mock.Of<IProductVersion>(x => x.GetBuildNumber() == serverHqBuildNumber);

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

            // Assert - should not be 406 (client matches server version)
            Assert.That(((IStatusCodeActionResult)result).StatusCode, Is.Not.EqualTo(StatusCodes.Status406NotAcceptable));
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
