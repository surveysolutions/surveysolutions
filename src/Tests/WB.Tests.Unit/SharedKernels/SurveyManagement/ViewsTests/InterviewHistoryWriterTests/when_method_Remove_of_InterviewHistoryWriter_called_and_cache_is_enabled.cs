using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ViewsTests.InterviewHistoryWriterTests
{
    internal class when_method_Remove_of_InterviewHistoryWriter_called_and_cache_is_enabled : InterviewHistoryWriterTestContext
    {
        Establish context = () =>
        {
            fileSystemAccessorMock = new Mock<IFileSystemAccessor>();
            fileSystemAccessorMock.Setup(x => x.IsFileExists(Moq.It.IsAny<string>())).Returns(true);

            interviewSummaryWriterMock = new Mock<IReadSideRepositoryWriter<InterviewSummary>>();
            interviewSummaryWriterMock.Setup(x => x.GetById(Moq.It.IsAny<string>()))
                .Returns(new InterviewSummary() { QuestionnaireId = questionnaireId, QuestionnaireVersion = questionnaireVersion, InterviewId = interviewId });
            interviewHistoryWriter = CreateInterviewHistoryWriter(interviewSummaryWriter: interviewSummaryWriterMock.Object, fileSystemAccessor: fileSystemAccessorMock.Object);
            interviewHistoryWriter.EnableCache();
        };

        Because of = () =>
            interviewHistoryWriter.Remove(interviewId.FormatGuid());

        It should_delete_file_with_interview_history = () =>
            fileSystemAccessorMock.Verify(x=>x.DeleteFile(Moq.It.IsAny<string>()), Times.Once);

        private static InterviewHistoryWriter interviewHistoryWriter;
        private static Mock<IReadSideRepositoryWriter<InterviewSummary>> interviewSummaryWriterMock;
        private static Mock<IFileSystemAccessor> fileSystemAccessorMock;
        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static Guid questionnaireId = Guid.Parse("22222222222222222222222222222222");
        private static long questionnaireVersion = 2;
    }
}
