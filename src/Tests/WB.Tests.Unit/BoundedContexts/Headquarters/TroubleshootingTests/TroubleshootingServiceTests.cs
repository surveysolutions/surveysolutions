using System;
using Microsoft.Practices.ServiceLocation;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Troubleshooting;
using WB.Core.BoundedContexts.Headquarters.Views.BrokenInterviewPackages;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.TroubleshootingTests
{
    [TestFixture]
    [TestOf(typeof(TroubleshootingService))]
    internal class TroubleshootingServiceTests
    {
        private string interviewKey = "11-11-11-11";
        private readonly Guid interviewId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private TestInMemoryWriter<InterviewSummary> interviewSummaryReader;
        private Mock<IQuestionnaireBrowseViewFactory> questionnaireFactoryMock;
        private Mock<IBrokenInterviewPackagesViewFactory> brokenPackagesFactoryMock;
        private Mock<IInterviewLogSummaryReader> syncLogFactoryMock;

        [SetUp]
        public void SetupTests()
        {
            var serviceLocator = Stub<IServiceLocator>.WithNotEmptyValues;
            ServiceLocator.SetLocatorProvider(() => serviceLocator);

            this.interviewSummaryReader = Stub.ReadSideRepository<InterviewSummary>();
            this.questionnaireFactoryMock = new Mock<IQuestionnaireBrowseViewFactory>();
            this.brokenPackagesFactoryMock = new Mock<IBrokenInterviewPackagesViewFactory>();
            this.syncLogFactoryMock = new Mock<IInterviewLogSummaryReader>();
        }

        [TearDown]
        public void CleanTests()
        {
        }

        [Test]
        public void When_interview_is_missing()
        {
            var service = Create.Service.Troubleshooting();
            var message = service.GetMissingDataReason(null, this.interviewKey);
            Assert.That(TroubleshootingMessages.NoData_NotFound.FormatString(interviewKey), Is.EqualTo(message));
        }

        [Test]
        public void When_interview_is_deleted_with_questionnaire()
        {
            interviewSummaryReader.Store(Create.Entity.InterviewSummary(interviewId, key: this.interviewKey, isDeleted: true), interviewKey);
            questionnaireFactoryMock.Setup(x => x.GetById(It.IsAny<QuestionnaireIdentity>()))
                .Returns(Create.Entity.QuestionnaireBrowseItem(deleted: true));

            var service = Create.Service.Troubleshooting(interviewSummaryReader, questionnaireFactoryMock.Object);
            var message = service.GetMissingDataReason(null, this.interviewKey);
            Assert.That(TroubleshootingMessages.NoData_QuestionnaireDeleted.FormatString(interviewKey), Is.EqualTo(message));
        }

        [Test]
        public void When_interview_is_deleted()
        {
            interviewSummaryReader.Store(Create.Entity.InterviewSummary(interviewId, key: this.interviewKey, isDeleted: true), interviewKey);
            questionnaireFactoryMock.Setup(x => x.GetById(It.IsAny<QuestionnaireIdentity>()))
                .Returns(Create.Entity.QuestionnaireBrowseItem(deleted: false));

            var service = Create.Service.Troubleshooting(interviewSummaryReader, questionnaireFactoryMock.Object);
            var message = service.GetMissingDataReason(null, this.interviewKey);
            Assert.That(TroubleshootingMessages.NoData_InterviewDeleted.FormatString(interviewKey), Is.EqualTo(message));
        }

        [Test]
        public void When_last_pushed_interview_package_was_broken_because_of_wrong_responsible()
        {
            interviewSummaryReader.Store(Create.Entity.InterviewSummary(interviewId, key: this.interviewKey), interviewKey);

            questionnaireFactoryMock.Setup(x => x.GetById(It.IsAny<QuestionnaireIdentity>()))
                .Returns(Create.Entity.QuestionnaireBrowseItem());
            brokenPackagesFactoryMock.Setup(x => x.GetLastInterviewBrokenPackage(interviewId))
                .Returns(Create.Entity.BrokenInterviewPackage(incomingDate: new DateTime(2017, 4, 4), exceptionType: InterviewDomainExceptionType.OtherUserIsResponsible));
            syncLogFactoryMock.Setup(x => x.GetInterviewLog(this.interviewId, It.IsAny<Guid>()))
                .Returns(Create.Entity.InterviewSyncLogSummary(lastUploadInterviewDate: new DateTime(2017, 4, 3)));

            var service = Create.Service.Troubleshooting(interviewSummaryReader, questionnaireFactoryMock.Object, syncLogFactoryMock.Object, brokenPackagesFactoryMock.Object);

            var message = service.GetMissingDataReason(null, this.interviewKey);
            Assert.That(TroubleshootingMessages.NoData_InterviewWasReassigned.FormatString(interviewKey), Is.EqualTo(message));
        }

        [Test]
        public void When_last_pushed_interview_package_was_broken_because_of_unexpected_error()
        {
            interviewSummaryReader.Store(Create.Entity.InterviewSummary(interviewId, key: this.interviewKey), interviewKey);

            questionnaireFactoryMock.Setup(x => x.GetById(It.IsAny<QuestionnaireIdentity>()))
                .Returns(Create.Entity.QuestionnaireBrowseItem());
            brokenPackagesFactoryMock.Setup(x => x.GetLastInterviewBrokenPackage(interviewId))
                .Returns(Create.Entity.BrokenInterviewPackage(incomingDate: new DateTime(2017, 4, 4), exceptionType: null));
            syncLogFactoryMock.Setup(x => x.GetInterviewLog(this.interviewId, It.IsAny<Guid>()))
                .Returns(Create.Entity.InterviewSyncLogSummary(lastUploadInterviewDate: new DateTime(2017, 4, 3)));

            var service = Create.Service.Troubleshooting(interviewSummaryReader, questionnaireFactoryMock.Object, syncLogFactoryMock.Object, brokenPackagesFactoryMock.Object);

            var message = service.GetMissingDataReason(null, this.interviewKey);
            Assert.That(TroubleshootingMessages.NoData_ContactSupport.FormatString(interviewKey), Is.EqualTo(message));
        }

        [Test]
        public void When_interview_status_is_completed_and_no_broken_packages()
        {
            interviewSummaryReader.Store(Create.Entity.InterviewSummary(interviewId, key: this.interviewKey, status: InterviewStatus.Completed), interviewKey);

            questionnaireFactoryMock.Setup(x => x.GetById(It.IsAny<QuestionnaireIdentity>()))
                .Returns(Create.Entity.QuestionnaireBrowseItem());
          
            syncLogFactoryMock.Setup(x => x.GetInterviewLog(this.interviewId, It.IsAny<Guid>()))
                .Returns(Create.Entity.InterviewSyncLogSummary(lastUploadInterviewDate: new DateTime(2017, 4, 3)));

            var service = Create.Service.Troubleshooting(interviewSummaryReader, questionnaireFactoryMock.Object, syncLogFactoryMock.Object, brokenPackagesFactoryMock.Object);

            var message = service.GetMissingDataReason(null, this.interviewKey);
            Assert.That(TroubleshootingMessages.NoData_NoIssuesInterviewOnServer.FormatString(interviewKey), Is.EqualTo(message));
        }

        [Test]
        public void When_interview_was_not_pulled_by_interviewer()
        {
            interviewSummaryReader.Store(Create.Entity.InterviewSummary(interviewId, key: this.interviewKey, responsibleName: "Vasya", status: InterviewStatus.InterviewerAssigned, receivedByInterviewer: false), interviewKey);

            questionnaireFactoryMock.Setup(x => x.GetById(It.IsAny<QuestionnaireIdentity>()))
                .Returns(Create.Entity.QuestionnaireBrowseItem());

            syncLogFactoryMock.Setup(x => x.GetInterviewLog(this.interviewId, It.IsAny<Guid>()))
                .Returns(Create.Entity.InterviewSyncLogSummary());

            var service = Create.Service.Troubleshooting(interviewSummaryReader, questionnaireFactoryMock.Object, syncLogFactoryMock.Object, brokenPackagesFactoryMock.Object);

            var message = service.GetMissingDataReason(null, this.interviewKey);
            Assert.That(TroubleshootingMessages.NoData_InterviewWasNotReceived.FormatString(interviewKey, "Vasya"), Is.EqualTo(message));
        }

        [Test]
        public void When_interviewer_has_not_pushed_interview_and_relink_was_happened()
        {
            interviewSummaryReader.Store(Create.Entity.InterviewSummary(interviewId, key: this.interviewKey,
                status: InterviewStatus.InterviewerAssigned, receivedByInterviewer: true), interviewKey);

            questionnaireFactoryMock.Setup(x => x.GetById(It.IsAny<QuestionnaireIdentity>()))
                .Returns(Create.Entity.QuestionnaireBrowseItem());

            syncLogFactoryMock.Setup(x => x.GetInterviewLog(this.interviewId, It.IsAny<Guid>()))
                .Returns(Create.Entity.InterviewSyncLogSummary(
                    firstDownloadInterviewDate: new DateTime(2017, 4, 1),
                    lastLinkDate: new DateTime(2017, 4, 2),
                    lastDownloadInterviewDate: new DateTime(2017, 4, 3),
                    lastUploadInterviewDate: null));

            var service = Create.Service.Troubleshooting(interviewSummaryReader, questionnaireFactoryMock.Object, syncLogFactoryMock.Object, brokenPackagesFactoryMock.Object);

            var message = service.GetMissingDataReason(null, this.interviewKey);

            Assert.That(TroubleshootingMessages.NoData_InterviewerChangedDevice.FormatString(interviewKey), Is.EqualTo(message));
        }

        [Test]
        public void When_tablet_was_relinked_after_interview_was_downloaded()
        {
            interviewSummaryReader.Store(Create.Entity.InterviewSummary(interviewId, key: this.interviewKey,
                status: InterviewStatus.InterviewerAssigned, receivedByInterviewer: true), interviewKey);

            questionnaireFactoryMock.Setup(x => x.GetById(It.IsAny<QuestionnaireIdentity>()))
                .Returns(Create.Entity.QuestionnaireBrowseItem());

            syncLogFactoryMock.Setup(x => x.GetInterviewLog(this.interviewId, It.IsAny<Guid>()))
                .Returns(Create.Entity.InterviewSyncLogSummary(
                    firstDownloadInterviewDate: new DateTime(2017, 4, 1),
                    lastUploadInterviewDate: new DateTime(2017, 4, 2),
                    lastDownloadInterviewDate: new DateTime(2017, 4, 3),
                    lastLinkDate: new DateTime(2017, 4, 4)));

            var service = Create.Service.Troubleshooting(interviewSummaryReader, questionnaireFactoryMock.Object, syncLogFactoryMock.Object, brokenPackagesFactoryMock.Object);

            var message = service.GetMissingDataReason(null, this.interviewKey);

            Assert.That(TroubleshootingMessages.NoData_InterviewerChangedDevice.FormatString(interviewKey), Is.EqualTo(message));
        }

        [Test]
        public void When_interview_was_is_on_device_and_was_not_pushed_yet()
        {
            interviewSummaryReader.Store(Create.Entity.InterviewSummary(interviewId, key: this.interviewKey,
                status: InterviewStatus.InterviewerAssigned, receivedByInterviewer: true), interviewKey);

            questionnaireFactoryMock.Setup(x => x.GetById(It.IsAny<QuestionnaireIdentity>()))
                .Returns(Create.Entity.QuestionnaireBrowseItem());

            syncLogFactoryMock.Setup(x => x.GetInterviewLog(this.interviewId, It.IsAny<Guid>()))
                .Returns(Create.Entity.InterviewSyncLogSummary(
                    lastLinkDate: new DateTime(2017, 4, 1),
                    firstDownloadInterviewDate: new DateTime(2017, 4, 2),
                    lastDownloadInterviewDate: new DateTime(2017, 4, 2),
                    lastUploadInterviewDate: null));

            var service = Create.Service.Troubleshooting(interviewSummaryReader, questionnaireFactoryMock.Object, syncLogFactoryMock.Object, brokenPackagesFactoryMock.Object);

            var message = service.GetMissingDataReason(null, this.interviewKey);

            Assert.That(TroubleshootingMessages.NoData_InterveiwWasNotUploadedYet.FormatString(interviewKey), Is.EqualTo(message));
        }

        [Test]
        public void When_interview_is_not_on_servert_but_was_uploaded()
        {
            interviewSummaryReader.Store(Create.Entity.InterviewSummary(interviewId, key: this.interviewKey,
                status: InterviewStatus.InterviewerAssigned, receivedByInterviewer: true), interviewKey);

            questionnaireFactoryMock.Setup(x => x.GetById(It.IsAny<QuestionnaireIdentity>()))
                .Returns(Create.Entity.QuestionnaireBrowseItem());

            syncLogFactoryMock.Setup(x => x.GetInterviewLog(this.interviewId, It.IsAny<Guid>()))
                .Returns(Create.Entity.InterviewSyncLogSummary(
                    lastLinkDate: new DateTime(2017, 4, 1),
                    firstDownloadInterviewDate: new DateTime(2017, 4, 2),
                    lastDownloadInterviewDate: new DateTime(2017, 4, 3),
                    lastUploadInterviewDate: new DateTime(2017, 4, 4)));

            var service = Create.Service.Troubleshooting(interviewSummaryReader, questionnaireFactoryMock.Object, syncLogFactoryMock.Object, brokenPackagesFactoryMock.Object);

            var message = service.GetMissingDataReason(null, this.interviewKey);

            Assert.That(TroubleshootingMessages.NoData_ContactSupportWithMoreDetails, Is.EqualTo(message));
        }

        [Test]
        public void When_there_is_no_broken_packages_with_census_interviews()
        {
            brokenPackagesFactoryMock
                .Setup(x => x.GetFilteredItems(It.IsAny<BrokenInterviewPackageFilter>()))
                .Returns(Create.Entity.BrokenInterviewPackagesView());

            var service = Create.Service.Troubleshooting(brokenPackagesFactory: brokenPackagesFactoryMock.Object);

            var message = service.GetCensusInterviewsMissingReason("aaaa$1", null, DateTime.Now.AddMonths(-1), DateTime.Now.AddDays(1));

            Assert.That(TroubleshootingMessages.MissingCensusInterviews_NoBrokenPackages_Message, Is.EqualTo(message));
        }

        [Test]
        public void When_there_is_broken_package_with_census_interview()
        {
            brokenPackagesFactoryMock
                .Setup(x => x.GetFilteredItems(It.IsAny<BrokenInterviewPackageFilter>()))
                .Returns(Create.Entity.BrokenInterviewPackagesView(Create.Entity.BrokenInterviewPackageView()));

            var service = Create.Service.Troubleshooting(brokenPackagesFactory: brokenPackagesFactoryMock.Object);

            var message = service.GetCensusInterviewsMissingReason("aaaa$1", null, DateTime.Now.AddMonths(-1), DateTime.Now.AddDays(1));

            Assert.That(TroubleshootingMessages.MissingCensusInterviews_SomeBrokenPackages_Message, Is.EqualTo(message));
        }
    }
}
