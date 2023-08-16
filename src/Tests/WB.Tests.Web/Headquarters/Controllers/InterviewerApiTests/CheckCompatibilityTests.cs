using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Net.Http.Headers;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.Implementation;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;
using WB.UI.Headquarters.Controllers.Api.DataCollection.Interviewer;

namespace WB.Tests.Web.Headquarters.Controllers.InterviewerApiTests
{
    [TestOf(nameof(InterviewerControllerBase.CheckCompatibility))]
    public class CheckCompatibilityTests
    {
        private const string InterviewerUserAgent = "org.worldbank.solutions.interviewer/{0} (QuestionnaireVersion/27.0.0)";

        [Test]
        public async Task when_user_is_linked_to_another_server_should_not_allow_to_synchronize()
        {
            var tenantSettings = new TestPlainStorage<ServerSettings>();
            tenantSettings.Store(new ServerSettings
            { 
                Id = ServerSettings.PublicTenantIdKey,
                Value = "server id"
            }, ServerSettings.PublicTenantIdKey);

            var controller = Create.Controller.InterviewerApiController(tenantSettings: tenantSettings);

            // Act 
            IStatusCodeActionResult response  = (IStatusCodeActionResult) await controller.CheckCompatibility("device", 7100, "client id");

            // Assert
            Assert.That(response, Has.Property(nameof(IStatusCodeActionResult.StatusCode)).EqualTo(StatusCodes.Status409Conflict));
        }

        [Test]
        public async Task when_resolved_comment_exists_and_client_has_7090_protocol_Should_not_allow_to_synchronize()
        {
            var interviewerVersionReader = new Mock<IInterviewerVersionReader>();
            interviewerVersionReader.Setup(x => x.InterviewerBuildNumber())
                .ReturnsAsync(0);
            
            var synchronizedUserId = Id.gA;
            var interviews =
                Mock.Of<IInterviewInformationFactory>(x => x.HasAnyInterviewsInProgressWithResolvedCommentsForInterviewer(synchronizedUserId) == true);

            var deviceId = "device";
            var authorizedUser = Mock.Of<IAuthorizedUser>(x => x.Id == synchronizedUserId);
            var userToDeviceService = Mock.Of<IUserToDeviceService>(x => x.GetLinkedDeviceId(synchronizedUserId) == deviceId);

            var interviewerApiController = Web.Create.Controller.InterviewerApiController(
                syncVersionProvider: new InterviewerSyncProtocolVersionProvider(),
                authorizedUser: authorizedUser,
                interviewerVersionReader: interviewerVersionReader.Object,
                userToDeviceService: userToDeviceService);

            // Act
            IActionResult httpResponseMessage = await interviewerApiController.CheckCompatibility(deviceId, 7090);

            // Assert
            Assert.That(httpResponseMessage, Is.InstanceOf<StatusCodeResult>());
            Assert.That(((StatusCodeResult)httpResponseMessage).StatusCode, Is.EqualTo(StatusCodes.Status426UpgradeRequired));
        }

        [Test]
        public async Task when_user_has_non_primary_workspace_assigned_Should_not_allow_to_synchronize_old_interviewer_version()
        {
            var authorizedUser = Mock.Of<IAuthorizedUser>(x => x.HasNonDefaultWorkspace == true);

            var interviewerApiController = Web.Create.Controller.InterviewerApiController(
                syncVersionProvider: new InterviewerSyncProtocolVersionProvider(),
                authorizedUser: authorizedUser);

            // Act
            IActionResult httpResponseMessage = await interviewerApiController.CheckCompatibility("device", 7100);

            // Assert
            AssertUpgradeRequired(httpResponseMessage);
        }
        
        [Test]
        public async Task when_assignment_has_audio_recording_enabled_should_force_client_upgrade()
        {
            var interviewerVersionReader = new Mock<IInterviewerVersionReader>();
            interviewerVersionReader.Setup(x => x.InterviewerBuildNumber())
                .ReturnsAsync(0);

            var synchronizedUserId = Id.gA;
            var assignments =
                Mock.Of<IAssignmentsService>(x => x.HasAssignmentWithAudioRecordingEnabled(synchronizedUserId) == true);

            var deviceId = "device";
            var authorizedUser = Mock.Of<IAuthorizedUser>(x => x.Id == synchronizedUserId);
            var userToDeviceService = Mock.Of<IUserToDeviceService>(x => x.GetLinkedDeviceId(synchronizedUserId) == deviceId);

            var interviewerApiController = Web.Create.Controller.InterviewerApiController(
                syncVersionProvider: new InterviewerSyncProtocolVersionProvider(),
                authorizedUser: authorizedUser,
                interviewerVersionReader: interviewerVersionReader.Object,
                userToDeviceService: userToDeviceService);

            // Act
            IActionResult httpResponseMessage = await interviewerApiController.CheckCompatibility(deviceId, 7080);

            // Assert
            AssertUpgradeRequired(httpResponseMessage);
        }

        private static void AssertUpgradeRequired(IActionResult httpResponseMessage)
        {
            Assert.That(httpResponseMessage, Is.InstanceOf<StatusCodeResult>());
            Assert.That(((StatusCodeResult) httpResponseMessage).StatusCode, Is.EqualTo(StatusCodes.Status426UpgradeRequired));
        }

        [TestCase(24699, "19.07.0.0 (build 24689)", HttpStatusCode.UpgradeRequired)]
        [TestCase(25689, "19.08.0.0 (build 25689)", HttpStatusCode.UpgradeRequired)]
        [TestCase(25679, "20.09.2 (build 25689)", HttpStatusCode.NotAcceptable)]
        [TestCase(26876, "20.01.0 (build 26876)", HttpStatusCode.UpgradeRequired)]
        [TestCase(26886, "20.01.1 (build 26886)", HttpStatusCode.UpgradeRequired)]
        [TestCase(26948, "20.01.2 (build 26948)", HttpStatusCode.UpgradeRequired)]
        [TestCase(26963, "20.01.3 (build 26963)", HttpStatusCode.UpgradeRequired)]
        public async Task when_set_setting_to_autoUpdateEnabled_to_false_should_return_correct_upgrade_result(
            int hqApkBuildNumber, string interviewerVersionFromClient, int result)
        {
            var interviewerUserAgent = string.Format(InterviewerUserAgent, interviewerVersionFromClient);
            var interviewerVersionReader = 
                new Mock<IInterviewerVersionReader>();
            interviewerVersionReader.Setup(x => x.InterviewerBuildNumber())
                .ReturnsAsync(hqApkBuildNumber);

            var deviceId = "device";
            var userToDeviceService = Mock.Of<IUserToDeviceService>(x => x.GetLinkedDeviceId(It.IsAny<Guid>()) == deviceId);

            var interviewerSettings = Abc.Create.Entity.InterviewerSettings(autoUpdateEnabled: false);
            var interviewerSettingsStorage = Mock.Of<IPlainKeyValueStorage<InterviewerSettings>>(m =>
                    m.GetById(AppSetting.InterviewerSettings) == interviewerSettings);

            var interviewerApiController = Web.Create.Controller.InterviewerApiController(
                userToDeviceService: userToDeviceService,
                interviewerSettings: interviewerSettingsStorage,
                interviewerVersionReader: interviewerVersionReader.Object);
            
            interviewerApiController.Request.Headers[HeaderNames.UserAgent] = interviewerUserAgent;

            // Act
            IActionResult httpResponseMessage = await interviewerApiController.CheckCompatibility(deviceId, 7060);

            var statusCodeResult = ((IStatusCodeActionResult) httpResponseMessage).StatusCode;
            if (statusCodeResult != null)
            {
                Assert.That(statusCodeResult, Is.EqualTo(result));
            }

            if (result == StatusCodes.Status200OK)
            {
                Assert.That(httpResponseMessage, Is.InstanceOf<JsonResult>());
            }
        }

        [TestCase(21699, "18.03.0.0 (build 21689)", HttpStatusCode.UpgradeRequired)]
        [TestCase(23689, "18.06.0.0 (build 23689)", HttpStatusCode.OK)]
        [TestCase(25689, "18.06.0.4 (build 25689)", HttpStatusCode.OK)]
        [TestCase(26688, "19.08.2 (build 26689)", HttpStatusCode.NotAcceptable)]
        [TestCase(26876, "20.01.0 (build 26876)", HttpStatusCode.UpgradeRequired)]
        [TestCase(26886, "20.01.1 (build 26886)", HttpStatusCode.UpgradeRequired)]
        [TestCase(26948, "20.01.2 (build 26948)", HttpStatusCode.UpgradeRequired)]
        [TestCase(26963, "20.01.3 (build 26963)", HttpStatusCode.UpgradeRequired)]
        public async Task when_set_setting_autoUpdateEnabled_to_true_should_return_correct_upgrade_result(
            int hqApkBuildNumber, string appVersion, int result)
        {
            var interviewerUserAgent = string.Format(InterviewerUserAgent, appVersion);

            var interviewerVersionReader = new Mock<IInterviewerVersionReader>();
            interviewerVersionReader.Setup(x => x.InterviewerBuildNumber())
                .ReturnsAsync(hqApkBuildNumber);                

            var deviceId = "device";
            var userToDeviceService = Mock.Of<IUserToDeviceService>(x => x.GetLinkedDeviceId(It.IsAny<Guid>()) == deviceId);

            var interviewerSettings = Abc.Create.Entity.InterviewerSettings(autoUpdateEnabled: true);
            var interviewerSettingsStorage = Mock.Of<IPlainKeyValueStorage<InterviewerSettings>>(m =>
                m.GetById(AppSetting.InterviewerSettings) == interviewerSettings);

            var interviewerApiController = Web.Create.Controller.InterviewerApiController(
                userToDeviceService: userToDeviceService,
                syncVersionProvider: new InterviewerSyncProtocolVersionProvider(),
                interviewerSettings: interviewerSettingsStorage,
                interviewerVersionReader: interviewerVersionReader.Object);

            interviewerApiController.Request.Headers[HeaderNames.UserAgent] = interviewerUserAgent;

            // Act
            var httpResponseMessage = await interviewerApiController.CheckCompatibility(deviceId, InterviewerSyncProtocolVersionProvider.ResolvedCommentsIntroduced);
            var statusCodeResult = ((IStatusCodeActionResult) httpResponseMessage).StatusCode;
            if (statusCodeResult != null)
            {
                Assert.That(statusCodeResult, Is.EqualTo(result));
            }

            if (result == 200)
            {
                Assert.That(httpResponseMessage, Is.InstanceOf<JsonResult>());
            }
        }
        
        [Test]
        public async Task when_user_has_old_interviewer_and_user_assigner_to_second_workspace_should_return_update_requared()
        {
            var interviewerVersionReader = new Mock<IInterviewerVersionReader>();
            interviewerVersionReader.Setup(x => x.InterviewerBuildNumber())
                .ReturnsAsync(0);
            
            var synchronizedUserId = Id.gA;

            var userWorkspaces = new List<string>()
            {
                "first",
                "second"
            };
            
            var deviceId = "device";
            var authorizedUser = Mock.Of<IAuthorizedUser>(x => 
                    x.Id == synchronizedUserId
                    && x.Workspaces == userWorkspaces 
            );
            var userToDeviceService = Mock.Of<IUserToDeviceService>(x => x.GetLinkedDeviceId(synchronizedUserId) == deviceId);

            var interviewerApiController = Web.Create.Controller.InterviewerApiController(
                syncVersionProvider: new InterviewerSyncProtocolVersionProvider(),
                authorizedUser: authorizedUser,
                interviewerVersionReader: interviewerVersionReader.Object,
                userToDeviceService: userToDeviceService);

            // Act
            IActionResult httpResponseMessage = await interviewerApiController.CheckCompatibility(deviceId, 7300);

            // Assert
            Assert.That(httpResponseMessage, Is.InstanceOf<StatusCodeResult>());
            Assert.That(((StatusCodeResult)httpResponseMessage).StatusCode, Is.EqualTo(StatusCodes.Status426UpgradeRequired));
        }
    }
}
