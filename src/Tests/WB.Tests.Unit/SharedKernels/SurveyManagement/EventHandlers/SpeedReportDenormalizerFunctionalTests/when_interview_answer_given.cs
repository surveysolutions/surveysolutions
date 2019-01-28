using System;
using System.Collections.Generic;
using Ncqrs.Eventing.ServiceModel.Bus;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reports.Factories;
using WB.Core.GenericSubdomains.Portable;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.SpeedReportDenormalizerFunctionalTests
{
    internal class when_interview_answer_given : SpeedReportDenormalizerFunctionalTestsContext
    {
        [Test]
        public void when_interview_first_answer_given_should_store_answer_date()
        {
            var interviewId = Guid.NewGuid();
            var createdDate = new DateTime(2019, 1, 17, 5, 34, 22, DateTimeKind.Utc);
            var firstAnswerDate = new DateTime(2019, 1, 17, 5, 37, 57, DateTimeKind.Utc);

            var interviewSummariesStorage = new TestInMemoryWriter<InterviewSummary>();
            var speedReportItemsStorage = new TestInMemoryWriter<SpeedReportInterviewItem>();
            var statusEventsToPublish = new List<IPublishableEvent>();

            statusEventsToPublish.Add(Create.PublishedEvent.InterviewCreated(interviewId: interviewId, originDate: createdDate));
            statusEventsToPublish.Add(Create.PublishedEvent.InterviewerAssigned(interviewId: interviewId));
            statusEventsToPublish.Add(Create.PublishedEvent.TextQuestionAnswered(interviewId: interviewId, originDate: firstAnswerDate));

            var denormalizer = CreateDenormalizer(interviewSummariesStorage, speedReportItemsStorage);

            foreach (var publishableEvent in statusEventsToPublish)
            {
                denormalizer.Handle(new[] { publishableEvent }, publishableEvent.EventSourceId);
            }

            var speedReportInterviewItem = speedReportItemsStorage.GetById(interviewId.FormatGuid());
            Assert.That(speedReportInterviewItem.InterviewId, Is.EqualTo(interviewId.FormatGuid()));
            Assert.That(speedReportInterviewItem.CreatedDate, Is.EqualTo(createdDate));
            Assert.That(speedReportInterviewItem.FirstAnswerDate.HasValue, Is.True);
            Assert.That(speedReportInterviewItem.FirstAnswerDate.Value, Is.EqualTo(firstAnswerDate));
        }
        
        [Test]
        public void when_interview_second_answer_given_should_store_first_answer_date()
        {
            var interviewId = Guid.NewGuid();
            var createdDate = new DateTime(2019, 1, 17, 5, 34, 22, DateTimeKind.Utc);
            var firstAnswerDate = new DateTime(2019, 1, 17, 5, 37, 57, DateTimeKind.Utc);

            var interviewSummariesStorage = new TestInMemoryWriter<InterviewSummary>();
            var speedReportItemsStorage = new TestInMemoryWriter<SpeedReportInterviewItem>();
            var statusEventsToPublish = new List<IPublishableEvent>();

            statusEventsToPublish.Add(Create.PublishedEvent.InterviewCreated(interviewId: interviewId, originDate: createdDate));
            statusEventsToPublish.Add(Create.PublishedEvent.SupervisorAssigned(interviewId: interviewId));
            statusEventsToPublish.Add(Create.PublishedEvent.InterviewerAssigned(interviewId: interviewId));
            statusEventsToPublish.Add(Create.PublishedEvent.DateTimeQuestionAnswered(interviewId: interviewId, originDate: firstAnswerDate));
            statusEventsToPublish.Add(Create.PublishedEvent.TextQuestionAnswered(interviewId: interviewId, originDate: DateTimeOffset.Now));

            var denormalizer = CreateDenormalizer(interviewSummariesStorage, speedReportItemsStorage);

            foreach (var publishableEvent in statusEventsToPublish)
            {
                denormalizer.Handle(new[] { publishableEvent }, publishableEvent.EventSourceId);
            }

            var speedReportInterviewItem = speedReportItemsStorage.GetById(interviewId.FormatGuid());
            Assert.That(speedReportInterviewItem.InterviewId, Is.EqualTo(interviewId.FormatGuid()));
            Assert.That(speedReportInterviewItem.CreatedDate, Is.EqualTo(createdDate));
            Assert.That(speedReportInterviewItem.FirstAnswerDate.HasValue, Is.True);
            Assert.That(speedReportInterviewItem.FirstAnswerDate.Value, Is.EqualTo(firstAnswerDate));

            Assert.That(speedReportInterviewItem.SupervisorName, Is.EqualTo("name"));
            Assert.That(speedReportInterviewItem.InterviewerName, Is.EqualTo("name"));
        }
    }
}
