using System;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewExportedDataEventHandlerTests
{
    internal class when_interview_completed_event_handled : InterviewExportedDataEventHandlerTestContext
    {
        Establish context = () =>
        {
            dataExportWriter = new Mock<IDataExportRepositoryWriter>();
            interviewExportedDataDenormalizer = CreateInterviewExportedDataDenormalizer( dataExportWriter: dataExportWriter.Object);
        };

        Because of = () => interviewExportedDataDenormalizer.Handle(Create.InterviewCompletedEvent(interviewId: interviewId));


        It should_record_complete_status =
           () => dataExportWriter.Verify(
               x =>
                   x.AddOrUpdateExportedDataByInterviewWithAction(interviewId, InterviewExportedAction.Completed), Times.Once);

        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static InterviewExportedDataDenormalizer interviewExportedDataDenormalizer;
        private static Mock<IDataExportRepositoryWriter> dataExportWriter;
    }
}