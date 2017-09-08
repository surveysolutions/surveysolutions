using System;
using System.Linq;
using Machine.Specifications;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.InterviewStatusTimeSpanDenormalizerTests
{
    [TestFixture(typeof(InterviewStatusTimeSpanDenormalizer))]
    internal class when_interview_approved_by_hq_event_recived
    {
        [SetUp]
        public void Establish()
        {
            interviewSummary =
                Create.Entity.InterviewSummary(interviewId: interviewId, 
                    statuses: new[]
                    {
                        Create.Entity.InterviewCommentedStatus(status: InterviewExportedAction.InterviewerAssigned, statusId: interviewId),
                        Create.Entity.InterviewCommentedStatus(status: InterviewExportedAction.FirstAnswerSet, statusId: interviewId)
                    });
            denormalizer = Create.Service.InterviewStatusTimeSpanDenormalizer();
        }

        [Test]
        public void should_record_ApprovedByHeadquarter_as_end_status()
        {
            denormalizer.Update(interviewSummary, Create.PublishedEvent.InterviewApprovedByHQ(interviewId: interviewId));
            interviewSummary.TimeSpansBetweenStatuses.First().EndStatus
                    .ShouldEqual(InterviewExportedAction.ApprovedByHeadquarter);
        }

        [Test]
        public void should_record_interviewer_assign_as_begin_status()
        {
            denormalizer.Update(interviewSummary, Create.PublishedEvent.InterviewApprovedByHQ(interviewId: interviewId));
            interviewSummary.TimeSpansBetweenStatuses.First().BeginStatus
                    .ShouldEqual(InterviewExportedAction.InterviewerAssigned);
        }

        private static InterviewStatusTimeSpanDenormalizer denormalizer;
        private static readonly Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static InterviewSummary interviewSummary;
         
    }
}