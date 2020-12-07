using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
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
using WB.Core.Infrastructure.Versions;
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
            var tenantSettings = new TestInMemoryKeyValueStorage<TenantSettings>();
            tenantSettings.Store(new TenantSettings{ TenantPublicId = "server id"}, AppSetting.TenantSettingsKey);

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
                interviewInformationFactory: interviews,
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
        public async Task when_no_assignments_and_device_has_protocol_version_7060_Should_allow_to_synchronize()
        {
            var syncProtocolVersionProvider = Mock.Of<IInterviewerSyncProtocolVersionProvider>(x =>
                x.GetProtocolVersion() == InterviewerSyncProtocolVersionProvider.ProtectedVariablesIntroduced);

            //var productVersion = Mock.Of<IProductVersion>(x => x.ToString() == "19.08.0.0 (build 0)");
            var interviewerVersionReader = new Mock<IInterviewerVersionReader>();
            interviewerVersionReader.Setup(x => x.InterviewerBuildNumber())
                .ReturnsAsync(0);

            var assignments =
                Mock.Of<IAssignmentsService>(x => x.GetAssignments(It.IsAny<Guid>()) == new List<Assignment>());

            var deviceId = "device";
            var userid = Id.g1;
            var authorizedUser = Mock.Of<IAuthorizedUser>(x => x.Id == userid);
            var userToDeviceService = Mock.Of<IUserToDeviceService>(x => x.GetLinkedDeviceId(userid) == deviceId);
            var interviewerSettings = Abc.Create.Entity.InterviewerSettings(autoUpdateEnabled: false);
            var interviewerSettingsStorage = Mock.Of<IPlainKeyValueStorage<InterviewerSettings>>(m =>
                m.GetById(AppSetting.InterviewerSettings) == interviewerSettings);

            var interviewerApiController = Web.Create.Controller.InterviewerApiController(syncVersionProvider: syncProtocolVersionProvider,
                assignmentsService: assignments,
                authorizedUser: authorizedUser,
                interviewerSettings: interviewerSettingsStorage,
                interviewerVersionReader: interviewerVersionReader.Object,
                userToDeviceService: userToDeviceService);
            interviewerApiController.Request.Headers[HeaderNames.UserAgent] =
                new ProductInfoHeaderValue("org.worldbank.solutions.interviewer", "19.08.0.0").ToString();

            // Act
            var httpResponseMessage = await interviewerApiController.CheckCompatibility(deviceId, 7060);

            Assert.That(httpResponseMessage, Is.InstanceOf<JsonResult>());
            Assert.That(((JsonResult)httpResponseMessage).Value, Is.EqualTo("449634775"));
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
                assignmentsService: assignments,
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

        [Test]
        public async Task when_assignment_has_no_audio_recording_user_should_be_able_synchronize()
        {
            var productVersion = Mock.Of<IProductVersion>(x => x.ToString() == "19.10.0.23853 (build 23853)");
            var interviewerVersionReader = 
                new Mock<IInterviewerVersionReader>();
            interviewerVersionReader.Setup(x => x.InterviewerBuildNumber())
                .ReturnsAsync(23853);

            var synchronizedUserId = Id.gA;

            var deviceId = "device";
            var authorizedUser = Mock.Of<IAuthorizedUser>(x => x.Id == synchronizedUserId);
            var userToDeviceService = Mock.Of<IUserToDeviceService>(x => x.GetLinkedDeviceId(synchronizedUserId) == deviceId);

            var interviewerApiController = Web.Create.Controller.InterviewerApiController(
                syncVersionProvider: new InterviewerSyncProtocolVersionProvider(),
                authorizedUser: authorizedUser,
                interviewerVersionReader: interviewerVersionReader.Object,
                userToDeviceService: userToDeviceService);

            // Act
            var httpResponseMessage = await interviewerApiController.CheckCompatibility(deviceId, 7080);

            // Assert
            Assert.That(httpResponseMessage, Is.InstanceOf<JsonResult>());
            Assert.That(((JsonResult)httpResponseMessage).Value, Is.EqualTo("449634775"));
        }

        [Test]
        public async Task when_assignment_has_protected_questions_and_client_doesnt_support_Should_require_upgrade()
        {
            var syncProtocolVersionProvider = Mock.Of<IInterviewerSyncProtocolVersionProvider>(x =>
                x.GetProtocolVersion() == InterviewerSyncProtocolVersionProvider.ProtectedVariablesIntroduced);

            var interviewerVersionReader =new Mock<IInterviewerVersionReader>();
            interviewerVersionReader.Setup(x => x.InterviewerBuildNumber())
                .ReturnsAsync(0);

            var assignments =
                Mock.Of<IAssignmentsService>(x => x.GetAssignments(It.IsAny<Guid>()) == new List<Assignment>() &&
                                                  x.HasAssignmentWithProtectedVariables(It.IsAny<Guid>()) == true);

            var deviceId = "device";
            var userId = Id.g1;
            var authorizedUser = Mock.Of<IAuthorizedUser>(x => x.Id == userId);
            var userToDeviceService = Mock.Of<IUserToDeviceService>(x => x.GetLinkedDeviceId(userId) == deviceId);

            var interviewerApiController = Web.Create.Controller.InterviewerApiController(syncVersionProvider: syncProtocolVersionProvider,
                assignmentsService: assignments,
                authorizedUser: authorizedUser,
                interviewerVersionReader: interviewerVersionReader.Object,
                userToDeviceService: userToDeviceService);
            interviewerApiController.Request.Headers[HeaderNames.UserAgent] =
                new ProductInfoHeaderValue("org.worldbank.solutions.interviewer", "18.04.0.0").ToString();

            // Act
            var httpResponseMessage = await interviewerApiController.CheckCompatibility(deviceId, 7060);

            AssertUpgradeRequired(httpResponseMessage);
        }

        [TestCase(24699, "19.07.0.0 (build 24689)", HttpStatusCode.UpgradeRequired)]
        [TestCase(25689, "19.08.0.0 (build 25689)", HttpStatusCode.OK)]
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
            var httpResponseMessage = await interviewerApiController.CheckCompatibility(deviceId, 7060);
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
    }
}
