using System;
using System.Linq;
using FluentAssertions;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.ServicesIntegration.Export;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.StatusChangeHistoryDenormalizerFunctionalTests
{
    internal class when_interview_second_answer_given : StatusChangeHistoryDenormalizerFunctionalTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            interviewStatuses = Create.Entity.InterviewSummary(statuses: new [] { Create.Entity.InterviewCommentedStatus(status: InterviewExportedAction.FirstAnswerSet, statusId: interviewId) } );
            denormalizer = CreateStatusChangeHistoryDenormalizerFunctional();
            BecauseOf();
        }

        public void BecauseOf() => result = denormalizer.Update(interviewStatuses, Create.PublishedEvent.TextQuestionAnswered(interviewId: Guid.NewGuid()));

        [NUnit.Framework.Test] public void should_record_first_answer_status () => result.InterviewCommentedStatuses.Last().Status.Should().Be(InterviewExportedAction.FirstAnswerSet);

        private static StatusChangeHistoryDenormalizerFunctional denormalizer;
        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static InterviewSummary interviewStatuses;
        private static InterviewSummary result;
    }
}
