using System;
using System.Collections.Generic;
using System.Linq;
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
    internal class when_handling_InterviewStatusChanged_events_and_there_are_no_last_status_for_this_interview
    {
        private readonly Guid interviewId = Id.g1;

        [SetUp]
        public void Establish()
        {
            cumulativeReportStatusChangeStorage = new TestInMemoryWriter<CumulativeReportStatusChange>();

            var existingEvents = new List<IPublishedEvent<InterviewStatusChanged>>
            {
                Create.PublishedEvent.InterviewStatusChanged(interviewId: interviewId,
                    status: InterviewStatus.InterviewerAssigned, eventId: Id.g2),
                Create.PublishedEvent.InterviewStatusChanged(interviewId: interviewId,
                    status: InterviewStatus.Completed, eventId: Id.g3),
                Create.PublishedEvent.InterviewStatusChanged(interviewId: interviewId,
                    status: InterviewStatus.RejectedBySupervisor, eventId: Id.g4),
            };

            var interviewReferences = Create.Entity.InterviewSummary(interviewId, questionnaireId: questionnaireId, questionnaireVersion: questionnaireVersion);
            var interviewReferencesStorage = new TestInMemoryWriter<InterviewSummary>(interviewId.FormatGuid(), interviewReferences);
            
            denormalizer = Create.Service.CumulativeChartDenormalizer(
                cumulativeReportReader: cumulativeReportStatusChangeStorage,
                cumulativeReportStatusChangeStorage: cumulativeReportStatusChangeStorage,
                interviewReferencesStorage: interviewReferencesStorage);

            denormalizer.Handle(existingEvents);
        }

        [Test]
        public void should_store_rejected_by_supervisor_state_minus_once()
        {
            var events = new[]
            {
                Create.PublishedEvent.InterviewStatusChanged(interviewId: interviewId,
                    status: InterviewStatus.Completed),
                Create.PublishedEvent.InterviewStatusChanged(interviewId: interviewId,
                    status: InterviewStatus.Restarted),
                Create.PublishedEvent.InterviewStatusChanged(interviewId: interviewId,
                    status: InterviewStatus.InterviewerAssigned),
                Create.PublishedEvent.InterviewStatusChanged(interviewId: interviewId,
                    status: InterviewStatus.Completed),
                Create.PublishedEvent.InterviewStatusChanged(interviewId: interviewId,
                    status: InterviewStatus.Restarted),
                Create.PublishedEvent.InterviewStatusChanged(interviewId: interviewId,
                    status: InterviewStatus.Completed)
            };

            denormalizer.Handle(events);

            var allChanges = cumulativeReportStatusChangeStorage.Dictionary.Select(v => v.Value).ToList();

            Assert.That(allChanges.Count(c => c.Status == InterviewStatus.RejectedBySupervisor && c.ChangeValue < 0), 
                Is.EqualTo(1));
        }

        private static CumulativeChartDenormalizer denormalizer;
        private static TestInMemoryWriter<CumulativeReportStatusChange> cumulativeReportStatusChangeStorage;
        private static readonly Guid questionnaireId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        private static long questionnaireVersion = 7112;
    }
}
