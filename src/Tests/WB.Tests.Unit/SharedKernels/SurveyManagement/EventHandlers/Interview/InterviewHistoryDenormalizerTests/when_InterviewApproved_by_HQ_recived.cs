using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewHistoryDenormalizerTests
{
    internal class when_InterviewApproved_by_HQ_recived : InterviewHistoryDenormalizerTestContext
    {
        Establish context = () =>
        {
            interviewHistoryView = CreateInterviewHistoryView();
            interviewExportedDataDenormalizer = CreateInterviewHistoryDenormalizer();
        };

        Because of = () =>
            interviewHistoryView = interviewExportedDataDenormalizer.Update(interviewHistoryView, CreatePublishableEvent(() => new InterviewApprovedByHQ(Guid.NewGuid(), "app"),
                interviewId));

        It should_action_of_first_record_be_ApproveByHeadquarter = () =>
            interviewHistoryView.Records[0].Action.ShouldEqual(InterviewHistoricalAction.ApproveByHeadquarter);


        private static InterviewHistoryDenormalizer interviewExportedDataDenormalizer;
        private static Guid interviewId = Guid.NewGuid();
        private static InterviewHistoryView interviewHistoryView;
    }
}
