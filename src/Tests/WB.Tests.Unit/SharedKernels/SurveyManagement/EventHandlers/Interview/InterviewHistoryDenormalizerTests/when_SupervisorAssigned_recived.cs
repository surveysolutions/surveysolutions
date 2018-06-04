using System;
using FluentAssertions;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.SharedKernels.DataCollection.Events.Interview;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewHistoryDenormalizerTests
{
    internal class when_SupervisorAssigned_recived : InterviewHistoryDenormalizerTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            interviewHistoryView = CreateInterviewHistoryView();
            interviewExportedDataDenormalizer = CreateInterviewHistoryDenormalizer();
            BecauseOf();
        }

        public void BecauseOf() =>
            interviewHistoryView = interviewExportedDataDenormalizer.Update(interviewHistoryView, CreatePublishableEvent(() => new SupervisorAssigned(Guid.NewGuid(), interviewId, DateTimeOffset.Now)));

        [NUnit.Framework.Test] public void should_action_of_first_record_be_SupervisorAssigned () =>
            interviewHistoryView.Records[0].Action.Should().Be(InterviewHistoricalAction.SupervisorAssigned);


        private static InterviewParaDataEventHandler interviewExportedDataDenormalizer;
        private static Guid interviewId = Guid.NewGuid();
        private static InterviewHistoryView interviewHistoryView;
    }
}
