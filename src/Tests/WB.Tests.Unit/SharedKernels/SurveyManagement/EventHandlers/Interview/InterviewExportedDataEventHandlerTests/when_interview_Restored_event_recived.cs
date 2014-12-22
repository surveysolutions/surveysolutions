using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewExportedDataEventHandlerTests
{
    internal class when_interview_Restored_event_recived : InterviewExportedDataEventHandlerTestContext
    {
        Establish context = () =>
        {
            dataExportService = new Mock<IDataExportRepositoryWriter>();

            interviewSummary = CreateInterviewSummary(new[]
            {
                InterviewStatus.Created,
                InterviewStatus.ApprovedByHeadquarters, InterviewStatus.SupervisorAssigned, 
                InterviewStatus.InterviewerAssigned, InterviewStatus.Completed, 
                InterviewStatus.Restarted, InterviewStatus.ApprovedBySupervisor, 
                InterviewStatus.RejectedBySupervisor, InterviewStatus.RejectedByHeadquarters
            });

            interviewExportedDataDenormalizer = CreateInterviewExportedDataEventHandlerForQuestionnarieCreatedByMethod(dataExportRepositoryWriter: dataExportService.Object, interviewSummary: interviewSummary);
        };

        Because of = () =>
            interviewExportedDataDenormalizer.Handle(CreatePublishableEvent(() => new InterviewRestored(Guid.NewGuid()),
                interviewId));

        It should_call_method_AddInterviewAction_of_dataExport_exacly_8_times = () =>
         dataExportService.Verify(x => x.AddInterviewAction(Moq.It.IsAny<InterviewExportedAction>(), interviewId, Moq.It.IsAny<Guid>(), Moq.It.IsAny<DateTime>()), Times.Exactly(8));

        It should_call_method_AddInterviewAction_of_dataExport_with_SupervisorAssigned_action = () =>
         dataExportService.Verify(x => x.AddInterviewAction(InterviewExportedAction.SupervisorAssigned, interviewId, Moq.It.IsAny<Guid>(), Moq.It.IsAny<DateTime>()), Times.Once);

        It should_call_method_AddInterviewAction_of_dataExport_with_InterviewerAssigned_action = () =>
         dataExportService.Verify(x => x.AddInterviewAction(InterviewExportedAction.InterviewerAssigned, interviewId, Moq.It.IsAny<Guid>(), Moq.It.IsAny<DateTime>()), Times.Once);

        It should_call_method_AddInterviewAction_of_dataExport_with_Completed_action = () =>
         dataExportService.Verify(x => x.AddInterviewAction(InterviewExportedAction.Completed, interviewId, Moq.It.IsAny<Guid>(), Moq.It.IsAny<DateTime>()), Times.Once);

        It should_call_method_AddInterviewAction_of_dataExport_with_RejectedBySupervisor_action = () =>
         dataExportService.Verify(x => x.AddInterviewAction(InterviewExportedAction.RejectedBySupervisor, interviewId, Moq.It.IsAny<Guid>(), Moq.It.IsAny<DateTime>()), Times.Once);

        It should_call_method_AddInterviewAction_of_dataExport_with_RejectedByHeadquarter_action = () =>
         dataExportService.Verify(x => x.AddInterviewAction(InterviewExportedAction.RejectedByHeadquarter, interviewId, Moq.It.IsAny<Guid>(), Moq.It.IsAny<DateTime>()), Times.Once);

        It should_call_method_AddInterviewAction_of_dataExport_with_ApproveBySupervisor_action = () =>
         dataExportService.Verify(x => x.AddInterviewAction(InterviewExportedAction.ApproveBySupervisor, interviewId, Moq.It.IsAny<Guid>(), Moq.It.IsAny<DateTime>()), Times.Once);

        It should_call_method_AddInterviewAction_of_dataExport_with_ApproveByHeadquarter_action = () =>
         dataExportService.Verify(x => x.AddInterviewAction(InterviewExportedAction.ApproveByHeadquarter, interviewId, Moq.It.IsAny<Guid>(), Moq.It.IsAny<DateTime>()), Times.Once);

        It should_call_method_AddInterviewAction_of_dataExport_with_Restarted_action = () =>
         dataExportService.Verify(x => x.AddInterviewAction(InterviewExportedAction.Restarted, interviewId, Moq.It.IsAny<Guid>(), Moq.It.IsAny<DateTime>()), Times.Once);

        private static InterviewExportedDataDenormalizer interviewExportedDataDenormalizer;
        private static Mock<IDataExportRepositoryWriter> dataExportService;
        private static Guid interviewId = Guid.NewGuid();
        private static InterviewSummary interviewSummary;
    }
}
