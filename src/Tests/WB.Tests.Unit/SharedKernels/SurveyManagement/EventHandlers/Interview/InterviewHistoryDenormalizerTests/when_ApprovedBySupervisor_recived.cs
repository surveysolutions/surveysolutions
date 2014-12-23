using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;
using WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewExportedDataEventHandlerTests;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewHistoryDenormalizerTests
{
    internal class when_ApprovedBySupervisor_recived : InterviewHistoryDenormalizerTestContext
    {
        Establish context = () =>
        {
            interviewHistoryView = CreateInterviewHistoryView();
            interviewExportedDataDenormalizer = CreateInterviewHistoryDenormalizer();
        };

        Because of = () =>
            interviewHistoryView= interviewExportedDataDenormalizer.Update(interviewHistoryView, CreatePublishableEvent(() => new InterviewApproved(Guid.NewGuid(), "comment"),
                interviewId));

        It should_action_of_first_record_be_ApproveBySupervisor = () =>
            interviewHistoryView.Records[0].Action.ShouldEqual(InterviewHistoricalAction.ApproveBySupervisor);


        private static InterviewHistoryDenormalizer interviewExportedDataDenormalizer;
        private static Guid interviewId = Guid.NewGuid();
        private static InterviewHistoryView interviewHistoryView;
    }
}
