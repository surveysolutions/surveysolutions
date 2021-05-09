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
    internal class when_interview_completed_event_received_after_restart
    {
        [SetUp]
        public void Establish()
        {
            interviewSummary =
                Create.Entity.InterviewSummary(interviewId: interviewId, 
                    statuses: new[]
                    {
                        Create.Entity.InterviewCommentedStatus(status: InterviewExportedAction.InterviewerAssigned, statusId: interviewId),
                        Create.Entity.InterviewCommentedStatus(status: InterviewExportedAction.FirstAnswerSet, statusId: interviewId),
                        Create.Entity.InterviewCommentedStatus(status: InterviewExportedAction.Completed, statusId: interviewId),
                        Create.Entity.InterviewCommentedStatus(status: InterviewExportedAction.Restarted, statusId: interviewId)
                    }, 
                    timeSpans: new[]
                    {
                        Create.Entity.TimeSpanBetweenStatuses(interviewerId: interviewId, endStatus: InterviewExportedAction.Completed)
                    });

            denormalizer = Create.Service.InterviewStatusTimeSpanDenormalizer();
        }

        [Test]
        public void should_contain_only_one_complete_record()
        {
            //act
            denormalizer.Update(interviewSummary, Create.PublishedEvent.InterviewCompleted(interviewId: interviewId));

            //assert
            interviewSummary.TimeSpansBetweenStatuses.Count(ts => ts.EndStatus == InterviewExportedAction.Completed).Should().Be(1);
        }

        [Test]
        public void should_record_interviewer_assign_as_begin_status()
        {
            //act
            denormalizer.Update(interviewSummary, Create.PublishedEvent.InterviewCompleted(interviewId: interviewId));

            //assert
            interviewSummary.TimeSpansBetweenStatuses.First().BeginStatus.Should().Be(InterviewExportedAction.InterviewerAssigned);
        }

        private static InterviewStatusTimeSpanDenormalizer denormalizer;
        private static readonly Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static InterviewSummary interviewSummary;

    }
}
