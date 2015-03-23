using System;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewHistoryDenormalizerTests
{
    internal class when_SupervisorAssigned_recived : InterviewHistoryDenormalizerTestContext
    {
        Establish context = () =>
        {
            interviewExportedDataDenormalizer = CreateInterviewHistoryDenormalizer();
        };

        Because of = () =>
            interviewHistoryView = interviewExportedDataDenormalizer.Update(null, CreatePublishableEvent(() => new SupervisorAssigned(Guid.NewGuid(), interviewId)));

        It should_action_of_first_record_be_SupervisorAssigned = () =>
            interviewHistoryView.Records[0].Action.ShouldEqual(InterviewHistoricalAction.SupervisorAssigned);


        private static InterviewHistoryDenormalizer interviewExportedDataDenormalizer;
        private static Guid interviewId = Guid.NewGuid();
        private static InterviewHistoryView interviewHistoryView;
    }
}
