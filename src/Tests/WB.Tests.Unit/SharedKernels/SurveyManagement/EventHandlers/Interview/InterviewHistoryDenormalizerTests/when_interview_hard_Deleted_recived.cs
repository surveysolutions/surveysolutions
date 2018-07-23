using System;
using FluentAssertions;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.SharedKernels.DataCollection.Events.Interview;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewHistoryDenormalizerTests
{
    internal class when_interview_hard_Deleted_recived : InterviewHistoryDenormalizerTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            interviewHistoryView = CreateInterviewHistoryView();
            interviewExportedDataDenormalizer = CreateInterviewHistoryDenormalizer();
            BecauseOf();
        }

        public void BecauseOf() =>
            interviewHistoryView = interviewExportedDataDenormalizer.Update(interviewHistoryView, CreatePublishableEvent(() => new InterviewHardDeleted(Guid.NewGuid(),DateTimeOffset.Now),
                interviewId));

        [NUnit.Framework.Test] public void should_history_should_be_null () =>
            interviewHistoryView.Should().BeNull();


        private static InterviewParaDataEventHandler interviewExportedDataDenormalizer;
        private static Guid interviewId = Guid.NewGuid();
        private static InterviewHistoryView interviewHistoryView;
    }
}
