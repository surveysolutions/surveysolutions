using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ViewsTests.InterviewHistoryWriterTests
{
    internal class when_method_Clear_of_InterviewHistoryWriter_called : InterviewHistoryWriterTestContext
    {
        Establish context = () =>
        {
            filebasedExportedDataAccessor = new Mock<IFilebasedExportedDataAccessor>();

            interviewHistoryWriter = CreateInterviewHistoryWriter(filebasedExportedDataAccessor: filebasedExportedDataAccessor.Object);
        };

        Because of = () =>
            interviewHistoryWriter.Clear();

        It should_call_CleanExportHistoryFolder = () =>
            filebasedExportedDataAccessor.Verify(x => x.CleanExportHistoryFolder(), Times.Once);
        
        private static InterviewHistoryWriter interviewHistoryWriter;
        private static Mock<IFilebasedExportedDataAccessor> filebasedExportedDataAccessor;
    }
}
