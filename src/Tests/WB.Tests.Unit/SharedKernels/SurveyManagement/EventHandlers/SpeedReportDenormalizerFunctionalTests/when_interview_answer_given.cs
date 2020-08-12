using System;
using System.Collections.Generic;
using Ncqrs.Eventing.ServiceModel.Bus;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
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
            var statusEventsToPublish = new List<IPublishableEvent>();

            statusEventsToPublish.Add(Create.PublishedEvent.InterviewCreated(interviewId: interviewId, originDate: createdDate));
            statusEventsToPublish.Add(Create.PublishedEvent.InterviewerAssigned(interviewId: interviewId));
            statusEventsToPublish.Add(Create.PublishedEvent.InterviewStatusChanged(interviewId, status: InterviewStatus.InterviewerAssigned));
            statusEventsToPublish.Add(Create.PublishedEvent.TextQuestionAnswered(interviewId: interviewId, originDate: firstAnswerDate));

            var denormalizer = CreateDenormalizer(interviewSummariesStorage);

            foreach (var publishableEvent in statusEventsToPublish)
            {
                denormalizer.Handle(new[] { publishableEvent });
            }

            var summary = interviewSummariesStorage.GetById(interviewId.FormatGuid());
            Assert.That(summary.CreatedDate, Is.EqualTo(createdDate));
            Assert.That(summary.FirstAnswerDate, Is.EqualTo(firstAnswerDate));
        }


        [Test]
        public void when_several_supervisors_interviewers_assigned_track_first_one()
        {
            var interviewId = Guid.NewGuid();
            var createdDate = new DateTime(2019, 1, 17, 5, 34, 22, DateTimeKind.Utc);
            var supervisor = Id.gA.ToString();
            var interviewer = Id.gB.ToString();

            var interviewSummariesStorage = new TestInMemoryWriter<InterviewSummary>();
            var statusEventsToPublish = new List<IPublishableEvent>();

            statusEventsToPublish.Add(Create.PublishedEvent.InterviewCreated(interviewId: interviewId, originDate: createdDate));
            
            // act  - assign first supervisor/interviewer. It should be stored in first one
            statusEventsToPublish.Add(Create.PublishedEvent.InterviewerAssigned(interviewId: interviewId, interviewerId: interviewer));
            statusEventsToPublish.Add(Create.PublishedEvent.SupervisorAssigned(interviewId: interviewId, supervisorId: supervisor));
            
            // act  - assign second supervisor. It should not be stored in first one
            statusEventsToPublish.Add(Create.PublishedEvent.SupervisorAssigned(interviewId: interviewId));
            statusEventsToPublish.Add(Create.PublishedEvent.InterviewerAssigned(interviewId: interviewId));
            
            var denormalizer = CreateDenormalizer(interviewSummariesStorage);

            foreach (var publishableEvent in statusEventsToPublish)
            {
                denormalizer.Handle(new[] { publishableEvent });
            }

            var summary = interviewSummariesStorage.GetById(interviewId.FormatGuid());

            Assert.That(summary.FirstSupervisorId.ToString(), Is.EqualTo(supervisor));
            Assert.That(summary.FirstInterviewerId.ToString(), Is.EqualTo(interviewer));
        }

        [Test]
        public void when_interview_second_answer_given_should_store_first_answer_date()
        {
            var interviewId = Guid.NewGuid();
            var createdDate = new DateTime(2019, 1, 17, 5, 34, 22, DateTimeKind.Utc);
            var firstAnswerDate = new DateTime(2019, 1, 17, 5, 37, 57, DateTimeKind.Utc);

            var interviewSummariesStorage = new TestInMemoryWriter<InterviewSummary>();
            var statusEventsToPublish = new List<IPublishableEvent>();

            statusEventsToPublish.Add(Create.PublishedEvent.InterviewCreated(interviewId: interviewId, originDate: createdDate));
            statusEventsToPublish.Add(Create.PublishedEvent.SupervisorAssigned(interviewId: interviewId));
            statusEventsToPublish.Add(Create.PublishedEvent.InterviewerAssigned(interviewId: interviewId));
            statusEventsToPublish.Add(Create.PublishedEvent.InterviewStatusChanged(interviewId, status: InterviewStatus.InterviewerAssigned));
            statusEventsToPublish.Add(Create.PublishedEvent.DateTimeQuestionAnswered(interviewId: interviewId, originDate: firstAnswerDate));
            statusEventsToPublish.Add(Create.PublishedEvent.TextQuestionAnswered(interviewId: interviewId, originDate: DateTimeOffset.Now));

            var denormalizer = CreateDenormalizer(interviewSummariesStorage);

            foreach (var publishableEvent in statusEventsToPublish)
            {
                denormalizer.Handle(new[] { publishableEvent });
            }

            var speedReportInterviewItem = interviewSummariesStorage.GetById(interviewId.FormatGuid());
            Assert.That(speedReportInterviewItem.CreatedDate, Is.EqualTo(createdDate));
            Assert.That(speedReportInterviewItem.FirstAnswerDate, Is.EqualTo(firstAnswerDate));

            Assert.That(speedReportInterviewItem.FirstSupervisorName, Is.EqualTo("name"));
            Assert.That(speedReportInterviewItem.FirstInterviewerName, Is.EqualTo("name"));
        }

         [Test]
        public void when_interview_answer_on_single_option_question_should_store_first_answer_date()
        {
            var interviewId = Guid.NewGuid();
            var createdDate = new DateTime(2019, 1, 17, 5, 34, 22, DateTimeKind.Utc);
            var firstAnswerDate = new DateTime(2019, 1, 17, 5, 37, 57, DateTimeKind.Utc);

            var interviewSummariesStorage = new TestInMemoryWriter<InterviewSummary>();
            var statusEventsToPublish = new List<IPublishableEvent>();

            statusEventsToPublish.Add(Create.PublishedEvent.InterviewCreated(interviewId: interviewId, originDate: createdDate));
            statusEventsToPublish.Add(Create.PublishedEvent.SupervisorAssigned(interviewId: interviewId));
            statusEventsToPublish.Add(Create.PublishedEvent.InterviewerAssigned(interviewId: interviewId));
            statusEventsToPublish.Add(Create.PublishedEvent.InterviewStatusChanged(interviewId, status: InterviewStatus.InterviewerAssigned));
            statusEventsToPublish.Add(Create.PublishedEvent.SingleOptionQuestionAnswered(interviewId, originDate: firstAnswerDate));
            
            var denormalizer = CreateDenormalizer(interviewSummariesStorage);

            foreach (var publishableEvent in statusEventsToPublish)
            {
                denormalizer.Handle(new[] { publishableEvent });
            }

            var speedReportInterviewItem = interviewSummariesStorage.GetById(interviewId.FormatGuid());
            Assert.That(speedReportInterviewItem.CreatedDate, Is.EqualTo(createdDate));
            Assert.That(speedReportInterviewItem.FirstAnswerDate, Is.EqualTo(firstAnswerDate));
        }
    }
}
