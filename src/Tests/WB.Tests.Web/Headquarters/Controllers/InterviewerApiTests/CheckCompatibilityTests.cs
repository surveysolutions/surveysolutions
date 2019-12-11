using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FluentAssertions.Common;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.Implementation;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Versions;
using WB.Core.SharedKernels.DataCollection;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

using WB.UI.Headquarters.API.DataCollection.Interviewer;

namespace WB.Tests.Unit.Applications.Headquarters.InterviewerApiTests
{
    [TestOf(nameof(InterviewerApiController.CheckCompatibility))]
    public class CheckCompatibilityTests
    {
        private const string InterviewerUserAgent = "org.worldbank.solutions.interviewer/{0} (QuestionnaireVersion/27.0.0)";

        [Test]
        public void when_user_is_linked_to_another_server_should_not_allow_to_synchronize()
        {
            var tenantSettings = new TestInMemoryKeyValueStorage<TenantSettings>();
            tenantSettings.Store(new TenantSettings{ TenantPublicId = "server id"}, AppSetting.TenantSettingsKey);

            var controller = Web.Create.Controller.InterviewerApiController(tenantSettings: tenantSettings);

            // Act 
            var response  = controller.CheckCompatibility("device", 1, "client id");

            // Assert
            Assert.That(response, Has.Property(nameof(HttpResponseMessage.StatusCode)).EqualTo(HttpStatusCode.Conflict));
        }

        [Test]
        public void when_resolved_comment_exists_and_client_has_7090_protocol_Should_not_allow_to_synchronize()
        {
            var productVersion = Mock.Of<IProductVersion>(x => x.ToString() == "18.06.0.0 (build 0)");

            var synchronizedUserId = Id.gA;
            var interviews =
                Mock.Of<IInterviewInformationFactory>(x => x.HasAnyInterviewsInProgressWithResolvedCommentsForInterviewer(synchronizedUserId) == true);

            var deviceId = "device";
            var authorizedUser = Mock.Of<IAuthorizedUser>(x => x.DeviceId == deviceId && x.Id == synchronizedUserId);

            var interviewerApiController = Web.Create.Controller.InterviewerApiController(
                syncVersionProvider: new InterviewerSyncProtocolVersionProvider(),
                productVersion: productVersion,
                interviewInformationFactory: interviews,
                authorizedUser: authorizedUser);

            // Act
            HttpResponseMessage httpResponseMessage = interviewerApiController.CheckCompatibility(deviceId, 7090);

            // Assert
            Assert.That(httpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.UpgradeRequired));
        }

        [Test]
        public void when_no_assignments_and_device_has_protocol_version_7060_Should_allow_to_synchronize()
        {
            var syncProtocolVersionProvider = Mock.Of<IInterviewerSyncProtocolVersionProvider>(x =>
                x.GetProtocolVersion() == InterviewerSyncProtocolVersionProvider.ProtectedVariablesIntroduced);

            var productVersion = Mock.Of<IProductVersion>(x => x.ToString() == "19.08.0.0 (build 0)");

            var assignments =
                Mock.Of<IAssignmentsService>(x => x.GetAssignments(It.IsAny<Guid>()) == new List<Assignment>());

            var deviceId = "device";
            var authorizedUser = Mock.Of<IAuthorizedUser>(x => x.DeviceId == deviceId);
            var interviewerSettings = Abc.Create.Entity.InterviewerSettings(autoUpdateEnabled: false);
            var interviewerSettingsStorage = Mock.Of<IPlainKeyValueStorage<InterviewerSettings>>(m =>
                m.GetById(AppSetting.InterviewerSettings) == interviewerSettings);

            var interviewerApiController = Web.Create.Controller.InterviewerApiController(syncVersionProvider: syncProtocolVersionProvider,
                productVersion: productVersion,
                assignmentsService: assignments,
                authorizedUser: authorizedUser,
                interviewerSettings: interviewerSettingsStorage);
            interviewerApiController.Request.Headers.UserAgent.Add(new ProductInfoHeaderValue("org.worldbank.solutions.interviewer", "19.08.0.0"));;

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

            var interviewerApiController = Web.Create.Controller.InterviewerApiController(
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
        public async Task when_assignment_has_no_audio_recording_user_should_be_able_synchronize()
        {
            var productVersion = Mock.Of<IProductVersion>(x => x.ToString() == "19.10.0.23853 (build 23853)");

            var synchronizedUserId = Id.gA;

            var deviceId = "device";
            var authorizedUser = Mock.Of<IAuthorizedUser>(x => x.DeviceId == deviceId && x.Id == synchronizedUserId);

            var interviewerApiController = Web.Create.Controller.InterviewerApiController(
                syncVersionProvider: new InterviewerSyncProtocolVersionProvider(),
                productVersion: productVersion,
                authorizedUser: authorizedUser);

            // Act
            HttpResponseMessage httpResponseMessage = interviewerApiController.CheckCompatibility(deviceId, 7080);

            // Assert
            Assert.That(httpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var response = await httpResponseMessage.Content.ReadAsStringAsync();
            Assert.That(response, Is.EqualTo("\"449634775\""));
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

            var interviewerApiController = Web.Create.Controller.InterviewerApiController(syncVersionProvider: syncProtocolVersionProvider,
                productVersion: productVersion,
                assignmentsService: assignments,
                authorizedUser: authorizedUser);
            interviewerApiController.Request.Headers.UserAgent.Add(new ProductInfoHeaderValue("org.worldbank.solutions.interviewer", "18.04.0.0"));;

            // Act
            HttpResponseMessage httpResponseMessage = interviewerApiController.CheckCompatibility(deviceId, 7060);

            Assert.That(httpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.UpgradeRequired));
        }

        [TestCase("19.12.0.0 (build 0)", "19.07.0.0", HttpStatusCode.UpgradeRequired)]
        [TestCase("19.12.0.0 (build 0)", "19.08.0.0", HttpStatusCode.OK)]
        [TestCase("19.10.0.0 (build 0)", "19.10.0.0", HttpStatusCode.OK)]
        [TestCase("19.10.0.1 (build 0)", "19.10.0.0", HttpStatusCode.OK)]
        [TestCase("19.10.0.4 (build 0)", "19.10.0.4", HttpStatusCode.OK)]
        [TestCase("20.09.0 (build 0)", "20.09.2 (build 25689)", HttpStatusCode.NotAcceptable)]
        public void when_set_setting_to_autoUpdateEnabled_to_false_should_return_correct_upgrade_result(
            string hqVersion, string appVersion, HttpStatusCode result)
        {
            var interviewerUserAgent = string.Format(InterviewerUserAgent, appVersion);
            var productVersionObj = Mock.Of<IProductVersion>(x => x.ToString() == hqVersion);

            var deviceId = "device";
            var authorizedUser = Mock.Of<IAuthorizedUser>(x => x.DeviceId == deviceId);

            var interviewerSettings = Abc.Create.Entity.InterviewerSettings(autoUpdateEnabled: false);
            var interviewerSettingsStorage = Mock.Of<IPlainKeyValueStorage<InterviewerSettings>>(m =>
                    m.GetById(AppSetting.InterviewerSettings) == interviewerSettings);

            var interviewerApiController = Web.Create.Controller.InterviewerApiController(
                productVersion: productVersionObj,
                authorizedUser: authorizedUser,
                interviewerSettings: interviewerSettingsStorage);
            interviewerApiController.Request.Headers.UserAgent.ParseAdd(interviewerUserAgent);

            // Act
            HttpResponseMessage httpResponseMessage = interviewerApiController.CheckCompatibility(deviceId, 7060);

            Assert.That(httpResponseMessage.StatusCode, Is.EqualTo(result));
        }

        [TestCase("18.12.0.0 (build 0)", "18.03.0.0", HttpStatusCode.UpgradeRequired)]
        [TestCase("18.12.0.0 (build 0)", "18.04.0.0", HttpStatusCode.UpgradeRequired)]
        [TestCase("18.06.0.0 (build 0)", "18.06.0.0", HttpStatusCode.OK)]
        [TestCase("18.06.0.1 (build 0)", "18.06.0.0", HttpStatusCode.UpgradeRequired)]
        [TestCase("18.06.0.4 (build 0)", "18.06.0.4", HttpStatusCode.OK)]
        [TestCase("19.08.0 (build 0)", "19.08.2 (build 25689)", HttpStatusCode.NotAcceptable)]
        public void when_set_setting_autoUpdateEnabled_to_true_should_return_correct_upgrade_result(
            string hqVersion, string appVersion, HttpStatusCode result)
        {
            var interviewerUserAgent = string.Format(InterviewerUserAgent, appVersion);

            var productVersionObj = Mock.Of<IProductVersion>(x => x.ToString() == hqVersion);

            var deviceId = "device";
            var authorizedUser = Mock.Of<IAuthorizedUser>(x => x.DeviceId == deviceId);

            var interviewerSettings = Abc.Create.Entity.InterviewerSettings(autoUpdateEnabled: true);
            var interviewerSettingsStorage = Mock.Of<IPlainKeyValueStorage<InterviewerSettings>>(m =>
                m.GetById(AppSetting.InterviewerSettings) == interviewerSettings);

            var interviewerApiController = Web.Create.Controller.InterviewerApiController(
                productVersion: productVersionObj,
                authorizedUser: authorizedUser,
                interviewerSettings: interviewerSettingsStorage);

            interviewerApiController.Request.Headers.UserAgent.ParseAdd(interviewerUserAgent);

            // Act
            HttpResponseMessage httpResponseMessage = interviewerApiController.CheckCompatibility(deviceId, 7060);

            Assert.That(httpResponseMessage.StatusCode, Is.EqualTo(result));
        }
    }
}
