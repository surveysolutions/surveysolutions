using System;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.CumulativeChartDenormalizerTests
{
    [TestFixture]
    internal class when_handling_InterviewStatusChanged_event_and_there_are_no_last_status_for_this_interview
    {
        [SetUp]
        public void Establish()
        {
            cumulativeReportStatusChangeStorageMock = new Mock<IReadSideRepositoryWriter<CumulativeReportStatusChange>>();
            @event = Create.PublishedEvent.InterviewStatusChanged(interviewId: Guid.Parse(interviewStringId), status: newStatus);
            
            var interviewReferences = Create.Entity.InterviewSummary(@event.EventSourceId, questionnaireId: questionnaireId, questionnaireVersion: questionnaireVersion);
            var interviewReferencesStorage = new TestInMemoryWriter<InterviewSummary>(@event.EventSourceId.FormatGuid(), interviewReferences);
            var cumulativeReportReader = new TestInMemoryWriter<CumulativeReportStatusChange>();

            denormalizer = Create.Service.CumulativeChartDenormalizer(
                cumulativeReportReader: cumulativeReportReader,
                cumulativeReportStatusChangeStorage: cumulativeReportStatusChangeStorageMock.Object,
                interviewReferencesStorage: interviewReferencesStorage);
        }

        [Test]
        public void should_not_store_interview_change_with_questionnaire_and_event_date_and_last_status_and_minus_one_value () 
        {
            denormalizer.Handle(@event);
            cumulativeReportStatusChangeStorageMock.Verify(storage =>
                    storage.Store(
                        It.Is<CumulativeReportStatusChange>(change =>
                            change.QuestionnaireIdentity == questionnaireIdentity &&
                            change.Date == @event.EventTimeStamp.Date &&
                            change.Status == lastStatus &&
                            change.ChangeValue == -1),
                        It.IsAny<string>()),
                Times.Never);
        }

        [Test]
        public void should_store_interview_change_with_questionnaire_and_event_date_and_new_status_and_plus_one_value()
        {
            denormalizer.Handle(@event);
            cumulativeReportStatusChangeStorageMock.Verify(storage =>
                    storage.Store(
                        It.Is<CumulativeReportStatusChange>(change =>
                            change.QuestionnaireIdentity == questionnaireIdentity &&
                            change.Date == @event.EventTimeStamp.Date &&
                            change.Status == newStatus &&
                            change.ChangeValue == +1),
                        It.IsAny<string>()),
                Times.Once);
        }

        private static CumulativeChartDenormalizer denormalizer;
        private static IPublishedEvent<InterviewStatusChanged> @event;
        private static Mock<IReadSideRepositoryWriter<CumulativeReportStatusChange>> cumulativeReportStatusChangeStorageMock;
        private static string interviewStringId = "11111111111111111111111111111111";
        private static InterviewStatus lastStatus = InterviewStatus.Completed;
        private static InterviewStatus newStatus = InterviewStatus.ApprovedBySupervisor;
        private static Guid questionnaireId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        private static string questionnaireIdentity = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa$7112";
        private static long questionnaireVersion = 7112;
    }
}