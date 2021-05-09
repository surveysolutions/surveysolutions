using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.ServicesIntegration.Export;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.InterviewStatusTimeSpanDenormalizerTests
{
    [TestFixture]
    internal class when_interview_completed_event_recived
    {
        [SetUp]
        public void Establish()
        {
            interviewStatuses =
                Create.Entity.InterviewSummary(interviewId: interviewId, statuses:
                    new[]
                    {
                        Create.Entity.InterviewCommentedStatus(status: InterviewExportedAction.InterviewerAssigned, statusId: interviewId),
                        Create.Entity.InterviewCommentedStatus(status: InterviewExportedAction.FirstAnswerSet, statusId: interviewId)
                    });
            
            denormalizer = Create.Service.InterviewStatusTimeSpanDenormalizer();
        }

        [Test]
        public void should_record_complete_as_end_status()
        {
            denormalizer.Update(interviewStatuses, Create.PublishedEvent.InterviewCompleted(interviewId: interviewId));
            interviewStatuses.TimeSpansBetweenStatuses.First().EndStatus
                    .Should().Be(InterviewExportedAction.Completed);
        }

        [Test]
        public void should_record_interviewer_assign_as_begin_status()
        {
        denormalizer.Update(interviewStatuses, Create.PublishedEvent.InterviewCompleted(interviewId: interviewId));
            interviewStatuses.TimeSpansBetweenStatuses.First().BeginStatus
                    .Should().Be(InterviewExportedAction.InterviewerAssigned);
        }

        private static InterviewStatusTimeSpanDenormalizer denormalizer;
        private static readonly Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static InterviewSummary interviewStatuses;
    }
}
