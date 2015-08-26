using System;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewExportedDataEventHandlerTests
{
    internal class when_interview_completed_event_handled_and_first_answer_set_event_is_missing_before_complete_but_present_in_future : InterviewExportedDataEventHandlerTestContext
    {
        Establish context = () =>
        {
            interviewCompletedEvent = Create.InterviewCompletedEvent(interviewId: interviewId);
            var interviewStatusesStorage = new TestInMemoryWriter<InterviewStatuses>();
            var interviewStatuses = Create.InterviewStatuses(interviewid: interviewId);
            interviewStatuses.InterviewCommentedStatuses.Add(Create.InterviewCommentedStatus(status: InterviewExportedAction.InterviewerAssigned));
            interviewStatuses.InterviewCommentedStatuses.Add(Create.InterviewCommentedStatus(status: InterviewExportedAction.Completed, statusId: interviewCompletedEvent.EventIdentifier));
            interviewStatuses.InterviewCommentedStatuses.Add(Create.InterviewCommentedStatus(status: InterviewExportedAction.Restored));
            interviewStatuses.InterviewCommentedStatuses.Add(Create.InterviewCommentedStatus(status: InterviewExportedAction.FirstAnswerSet));
            interviewStatuses.InterviewCommentedStatuses.Add(Create.InterviewCommentedStatus(status: InterviewExportedAction.Completed));
            interviewStatusesStorage.Store(interviewStatuses, interviewId);

            dataExportWriter = new Mock<IDataExportRepositoryWriter>();
            interviewExportedDataDenormalizer = CreateInterviewExportedDataDenormalizer(statuses: interviewStatusesStorage, dataExportWriter: dataExportWriter.Object);
        };

        Because of = () => interviewExportedDataDenormalizer.Handle(interviewCompletedEvent);

        It should_not_record_first_answer_status =
            () => dataExportWriter.Verify(
                x =>
                    x.AddInterviewAction(InterviewExportedAction.FirstAnswerSet, interviewId,
                        Moq.It.IsAny<Guid>(), Moq.It.IsAny<DateTime>()), Times.Never);

        It should_record_complete_status =
           () => dataExportWriter.Verify(
               x =>
                   x.AddInterviewAction(InterviewExportedAction.Completed, interviewId,
                       Moq.It.IsAny<Guid>(), Moq.It.IsAny<DateTime>()), Times.Once);

        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static InterviewExportedDataDenormalizer interviewExportedDataDenormalizer;
        private static Mock<IDataExportRepositoryWriter> dataExportWriter;
        private static IPublishedEvent<InterviewCompleted> interviewCompletedEvent;
    }
}