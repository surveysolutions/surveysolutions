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
    internal class when_interview_unapproved_by_headquarters_event_recived
    {
        [SetUp]
        public void Establish()
        {
            interviewSummary = Create.Entity.InterviewSummary(
                questionnaireId: questionnaireId,
                questionnaireVersion: 1,
                interviewId: interviewId,
                timeSpans: new[]
                {
                    Create.Entity.TimeSpanBetweenStatuses(interviewerId: userId,
                        timestamp: DateTime.Now.AddHours(1),
                        timeSpanWithPreviousStatus: TimeSpan.FromMinutes(-35))
                });
        
            
            denormalizer = Create.Service.InterviewStatusTimeSpanDenormalizer();
        }

        [Test]
        public void should_remove_ApprovedByHeadquarter_as_end_status ()
        {
            //act
            denormalizer.Update(interviewSummary, Create.PublishedEvent.UnapprovedByHeadquarters(interviewId: interviewId));

            //assert
            interviewSummary.TimeSpansBetweenStatuses.Count(x => x.EndStatus == InterviewExportedAction.ApprovedByHeadquarter).ShouldEqual(0);
        }

        private static InterviewStatusTimeSpanDenormalizer denormalizer;
        private static readonly Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid questionnaireId = Guid.Parse("21111111111111111111111111111111");
        private static readonly Guid userId = Guid.Parse("31111111111111111111111111111111");
        private static InterviewSummary interviewSummary;

    }
}