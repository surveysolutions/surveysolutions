using System;
using System.Linq;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Infrastructure.Native.Storage;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.CumulativeChartDenormalizerTests
{
    [TestFixture]
    internal class when_handling_InterviewStatusChanged_event_and_there_is_last_status_for_this_interview_and_event_has_previous_status
    {
        [SetUp]
        public void Establish()
        {
            cumulativeReportStatusChangeStorage = new Mock<INativeReadSideStorage<CumulativeReportStatusChange>>();

            cumulativeReportStatusChangeWriter = new TestInMemoryWriter<CumulativeReportStatusChange>(
                Guid.NewGuid().FormatGuid(),
                Create.Entity.CumulativeReportStatusChange("id", questionnaireId, questionnaireVersion, new DateTime(2017, 1, 30), previousStatus, 1, Guid.Parse(interviewStringId), 88));


            @event = Create.PublishedEvent.InterviewStatusChanged(
                interviewId: Guid.Parse(interviewStringId),
                status: InterviewStatus.ApprovedBySupervisor,
                previousStatus: previousStatus,
                eventId: Id.g9);

            var interviewReferences = Create.Entity.InterviewSummary(@event.EventSourceId, questionnaireId: questionnaireId, questionnaireVersion: questionnaireVersion);
            var interviewReferencesStorage = new TestInMemoryWriter<InterviewSummary>(@event.EventSourceId.FormatGuid(), interviewReferences);

            denormalizer = Create.Service.CumulativeChartDenormalizer(
                cumulativeReportReader: cumulativeReportStatusChangeStorage.Object,
                cumulativeReportStatusChangeStorage: cumulativeReportStatusChangeWriter,
                interviewReferencesStorage: interviewReferencesStorage);
        }


        [Test]
        public void should_store_interview_change_with_questionnaire_and_event_date_and_last_status_and_minus_one_value()
        {
            //act
            denormalizer.Handle(@event.ToEnumerable());

            //assert
            cumulativeReportStatusChangeStorage.Verify(x => x.Query(It.IsAny<Func<IQueryable<CumulativeReportStatusChange>, CumulativeReportStatusChange>>()), Times.Never);
        }

        [Test]
        public void should_store_interview_change_with_questionnaire_and_event_date_and_new_status_and_plus_one_value()
        {
            //act
            denormalizer.Handle(@event.ToEnumerable());

            //assert
            cumulativeReportStatusChangeStorage.Verify(x => x.Query(It.IsAny<Func<IQueryable<CumulativeReportStatusChange>, CumulativeReportStatusChange>>()), Times.Never);
        }

        private static CumulativeChartDenormalizer denormalizer;
        private static IPublishedEvent<InterviewStatusChanged> @event;
        private static Mock<INativeReadSideStorage<CumulativeReportStatusChange>> cumulativeReportStatusChangeStorage;
        private static string interviewStringId = "11111111111111111111111111111111";
        private static InterviewStatus previousStatus = InterviewStatus.Completed;
        private static Guid questionnaireId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        private static long questionnaireVersion = 7112;
        private TestInMemoryWriter<CumulativeReportStatusChange> cumulativeReportStatusChangeWriter;
    }
}
