using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Versions;
using WB.Core.SharedKernels.DataCollection;
using WB.Tests.Abc;
using WB.UI.Headquarters.API.DataCollection;
using WB.UI.Headquarters.API.DataCollection.Interviewer;

namespace WB.Tests.Unit.Applications.Headquarters.InterviewerApiTests
{
    [TestOf(nameof(InterviewerApiController.CheckCompatibility))]
    public class CheckCompatibilityTests
    {
        [Test]
        public void when_no_assignments_and_device_has_protocol_version_7060_Should_allow_to_synchronize()
        {
            var syncProtocolVersionProvider = Mock.Of<IInterviewerSyncProtocolVersionProvider>(x =>
                x.GetProtocolVersion() == InterviewerSyncProtocolVersionProvider.ProtectedVariablesIntroduced);

            var productVersion = Mock.Of<IProductVersion>(x => x.ToString() == "18.06.0.0 (build 0)");

            var assignments =
                Mock.Of<IAssignmentsService>(x => x.GetAssignments(It.IsAny<Guid>()) == new List<Assignment>());

            var deviceId = "device";
            var authorizedUser = Mock.Of<IAuthorizedUser>(x => x.DeviceId == deviceId);
            var interviewerSettings = Create.Entity.InterviewerSettings(autoUpdateEnabled: false);
            var interviewerSettingsStorage = Mock.Of<IPlainKeyValueStorage<InterviewerSettings>>(m =>
                m.GetById(AppSetting.InterviewerSettings) == interviewerSettings);

            var interviewerApiController = Create.Controller.InterviewerApiController(syncVersionProvider: syncProtocolVersionProvider,
                productVersion: productVersion,
                assignmentsService: assignments,
                authorizedUser: authorizedUser,
                interviewerSettings: interviewerSettingsStorage);
            interviewerApiController.Request.Headers.UserAgent.Add(new ProductInfoHeaderValue("org.worldbank.solutions.interviewer", "18.04.0.0"));;

            // Act
            HttpResponseMessage httpResponseMessage = interviewerApiController.CheckCompatibility(deviceId, 7060);

            Assert.That(httpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public void when_assignment_has_audio_recording_enabled_should_force_client_upgrade()
        {
            var productVersion = Mock.Of<IProductVersion>(x => x.ToString() == "18.06.0.0 (build 0)");

            var synchronizedUserId = Id.gA;
            var assignments =
                Mock.Of<IAssignmentsService>(x => x.HasAssignmentWithAudioRecordingEnabled(synchronizedUserId) == true);

            var deviceId = "device";
            var authorizedUser = Mock.Of<IAuthorizedUser>(x => x.DeviceId == deviceId && x.Id == synchronizedUserId);

            var interviewerApiController = Create.Controller.InterviewerApiController(
                syncVersionProvider: new InterviewerSyncProtocolVersionProvider(),
                productVersion: productVersion,
                assignmentsService: assignments,
                authorizedUser: authorizedUser);

            // Act
            HttpResponseMessage httpResponseMessage = interviewerApiController.CheckCompatibility(deviceId, 7080);

            // Assert
            Assert.That(httpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.UpgradeRequired));
        }

        [Test]
        public void when_assignment_has_protected_questions_and_client_doesnt_support_Should_require_upgrade()
        {
            var syncProtocolVersionProvider = Mock.Of<IInterviewerSyncProtocolVersionProvider>(x =>
                x.GetProtocolVersion() == InterviewerSyncProtocolVersionProvider.ProtectedVariablesIntroduced);

            var productVersion = Mock.Of<IProductVersion>(x => x.ToString() == "18.06.0.0 (build 0)");

            var assignments =
                Mock.Of<IAssignmentsService>(x => x.GetAssignments(It.IsAny<Guid>()) == new List<Assignment>() &&
                                                  x.HasAssignmentWithProtectedVariables(It.IsAny<Guid>()) == true);

            var deviceId = "device";
            var authorizedUser = Mock.Of<IAuthorizedUser>(x => x.DeviceId == deviceId);

            var interviewerApiController = Create.Controller.InterviewerApiController(syncVersionProvider: syncProtocolVersionProvider,
                productVersion: productVersion,
                assignmentsService: assignments,
                authorizedUser: authorizedUser);
            interviewerApiController.Request.Headers.UserAgent.Add(new ProductInfoHeaderValue("org.worldbank.solutions.interviewer", "18.04.0.0"));;

            // Act
            HttpResponseMessage httpResponseMessage = interviewerApiController.CheckCompatibility(deviceId, 7060);

            Assert.That(httpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.UpgradeRequired));
        }

        [TestCase("18.12.0.0 (build 0)", "18.03.0.0", HttpStatusCode.UpgradeRequired)]
        [TestCase("18.12.0.0 (build 0)", "18.04.0.0", HttpStatusCode.OK)]
        [TestCase("18.06.0.0 (build 0)", "18.06.0.0", HttpStatusCode.OK)]
        [TestCase("18.06.0.1 (build 0)", "18.06.0.0", HttpStatusCode.OK)]
        [TestCase("18.06.0.4 (build 0)", "18.06.0.4", HttpStatusCode.OK)]
        public void when_set_setting_to_autoUpdateEnabled_to_false_should_return_correct_upgrade_result(
            string hqVersion, string appVersion, HttpStatusCode result)
        {
            var productVersionObj = Mock.Of<IProductVersion>(x => x.ToString() == hqVersion);

            var deviceId = "device";
            var authorizedUser = Mock.Of<IAuthorizedUser>(x => x.DeviceId == deviceId);

            var interviewerSettings = Create.Entity.InterviewerSettings(autoUpdateEnabled: false);
            var interviewerSettingsStorage = Mock.Of<IPlainKeyValueStorage<InterviewerSettings>>(m =>
                    m.GetById(AppSetting.InterviewerSettings) == interviewerSettings);

            var interviewerApiController = Create.Controller.InterviewerApiController(
                productVersion: productVersionObj,
                authorizedUser: authorizedUser,
                interviewerSettings: interviewerSettingsStorage);
            interviewerApiController.Request.Headers.UserAgent.Add(new ProductInfoHeaderValue("org.worldbank.solutions.interviewer", appVersion));;

            // Act
            HttpResponseMessage httpResponseMessage = interviewerApiController.CheckCompatibility(deviceId, 7060);

            Assert.That(httpResponseMessage.StatusCode, Is.EqualTo(result));
        }

        [TestCase("18.12.0.0 (build 0)", "18.03.0.0", HttpStatusCode.UpgradeRequired)]
        [TestCase("18.12.0.0 (build 0)", "18.04.0.0", HttpStatusCode.UpgradeRequired)]
        [TestCase("18.06.0.0 (build 0)", "18.06.0.0", HttpStatusCode.OK)]
        [TestCase("18.06.0.1 (build 0)", "18.06.0.0", HttpStatusCode.UpgradeRequired)]
        [TestCase("18.06.0.4 (build 0)", "18.06.0.4", HttpStatusCode.OK)]
        public void when_set_setting_autoUpdateEnabled_to_true_should_return_correct_upgrade_result(
            string hqVersion, string appVersion, HttpStatusCode result)
        {
            var productVersionObj = Mock.Of<IProductVersion>(x => x.ToString() == hqVersion);

            var deviceId = "device";
            var authorizedUser = Mock.Of<IAuthorizedUser>(x => x.DeviceId == deviceId);

            var interviewerSettings = Create.Entity.InterviewerSettings(autoUpdateEnabled: true);
            var interviewerSettingsStorage = Mock.Of<IPlainKeyValueStorage<InterviewerSettings>>(m =>
                m.GetById(AppSetting.InterviewerSettings) == interviewerSettings);

            var interviewerApiController = Create.Controller.InterviewerApiController(
                productVersion: productVersionObj,
                authorizedUser: authorizedUser,
                interviewerSettings: interviewerSettingsStorage);
            interviewerApiController.Request.Headers.UserAgent.Add(new ProductInfoHeaderValue("org.worldbank.solutions.interviewer", appVersion)); ;

            // Act
            HttpResponseMessage httpResponseMessage = interviewerApiController.CheckCompatibility(deviceId, 7060);

            Assert.That(httpResponseMessage.StatusCode, Is.EqualTo(result));
        }

    }
}
