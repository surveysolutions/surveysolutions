using System;
using Ncqrs.Eventing.ServiceModel.Bus;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
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
            cumulativeReportStatusChangeStorage = new TestInMemoryWriter<CumulativeReportStatusChange>();
            @event = Create.PublishedEvent.InterviewStatusChanged(interviewId: Guid.Parse(interviewStringId), status: newStatus, eventId: Id.g9);
            
            var interviewReferences = Create.Entity.InterviewSummary(@event.EventSourceId, questionnaireId: questionnaireId, questionnaireVersion: questionnaireVersion);
            var interviewReferencesStorage = new TestInMemoryWriter<InterviewSummary>(@event.EventSourceId.FormatGuid(), interviewReferences);
            var cumulativeReportReader = new TestInMemoryWriter<CumulativeReportStatusChange>();

            denormalizer = Create.Service.CumulativeChartDenormalizer(
                cumulativeReportReader: cumulativeReportReader,
                cumulativeReportStatusChangeStorage: cumulativeReportStatusChangeStorage,
                interviewReferencesStorage: interviewReferencesStorage);
        }

        [Test]
        public void should_not_store_interview_change_with_questionnaire_and_event_date_and_last_status_and_minus_one_value () 
        {
            denormalizer.Handle(@event.ToEnumerable());
            var minusKey = $"{@Id.g9.FormatGuid()}-minus";
            Assert.That(cumulativeReportStatusChangeStorage.GetById(minusKey), Is.Null);
        }

        [Test]
        public void should_store_interview_change_with_questionnaire_and_event_date_and_new_status_and_plus_one_value()
        {
            denormalizer.Handle(@event.ToEnumerable());
            var plusKey = $"{@Id.g9.FormatGuid()}-plus";
            var plusRecord = cumulativeReportStatusChangeStorage.GetById(plusKey);
            Assert.That(plusRecord, Is.Not.Null);
            Assert.That(plusRecord.QuestionnaireIdentity, Is.EqualTo(questionnaireIdentity));
            Assert.That(plusRecord.Date, Is.EqualTo(@event.EventTimeStamp.Date));
            Assert.That(plusRecord.Status, Is.EqualTo(newStatus));
            Assert.That(plusRecord.ChangeValue, Is.EqualTo(+1));
        }

        private static CumulativeChartDenormalizer denormalizer;
        private static IPublishedEvent<InterviewStatusChanged> @event;
        private static TestInMemoryWriter<CumulativeReportStatusChange> cumulativeReportStatusChangeStorage;
        private static string interviewStringId = "11111111111111111111111111111111";
        private static InterviewStatus newStatus = InterviewStatus.ApprovedBySupervisor;
        private static readonly Guid questionnaireId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        private static string questionnaireIdentity = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa$7112";
        private static long questionnaireVersion = 7112;
    }
}
