using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewHistoryDenormalizerTests
{
    internal class when_interview_hard_Deleted_recived : InterviewHistoryDenormalizerTestContext
    {
        Establish context = () =>
        {
            interviewHistoryView = CreateInterviewHistoryView();
            interviewExportedDataDenormalizer = CreateInterviewHistoryDenormalizer();
        };

        Because of = () =>
            interviewHistoryView = interviewExportedDataDenormalizer.Update(interviewHistoryView, CreatePublishableEvent(() => new InterviewHardDeleted(Guid.NewGuid()),
                interviewId));

        It should_history_should_be_null = () =>
            interviewHistoryView.ShouldBeNull();


        private static InterviewParaDataEventHandler interviewExportedDataDenormalizer;
        private static Guid interviewId = Guid.NewGuid();
        private static InterviewHistoryView interviewHistoryView;
    }
}
