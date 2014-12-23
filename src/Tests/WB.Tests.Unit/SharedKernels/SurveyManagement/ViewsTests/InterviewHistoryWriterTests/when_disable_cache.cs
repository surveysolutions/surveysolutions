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
    internal class when_disable_cache : InterviewHistoryWriterTestContext
    {
        Establish context = () =>
        {
            fileSystemAccessorMock = new Mock<IFileSystemAccessor>();
            fileSystemAccessorMock.Setup(x => x.IsFileExists(Moq.It.IsAny<string>())).Returns(true);
            fileSystemAccessorMock.Setup(x => x.IsDirectoryExists(Moq.It.IsAny<string>())).Returns(true);

            interviewHistoryView = CreateInterviewHistoryView();
            interviewHistoryView.Records.Add(CreateInterviewHistoricalRecordView());

            interviewSummaryWriterMock = new Mock<IReadSideRepositoryWriter<InterviewSummary>>();
            interviewSummaryWriterMock.Setup(x => x.GetById(Moq.It.IsAny<string>()))
                .Returns(new InterviewSummary() { QuestionnaireId = questionnaireId, QuestionnaireVersion = questionnaireVersion, InterviewId = interviewId });

            interviewHistoryWriter = CreateInterviewHistoryWriter(interviewSummaryWriter: interviewSummaryWriterMock.Object, fileSystemAccessor: fileSystemAccessorMock.Object);
            interviewHistoryWriter.EnableCache();
            interviewHistoryWriter.Store(interviewHistoryView, interviewHistoryView.InterviewId.FormatGuid());
        };

        Because of = () =>
            interviewHistoryWriter.DisableCache();

        It should_create_file_with_interview_history = () =>
            fileSystemAccessorMock.Verify(x => x.OpenOrCreateFile(Moq.It.IsAny<string>(), Moq.It.IsAny<bool>()), Times.Once);

        It should_return_readable_status_with_disabled_cache = () =>
            interviewHistoryWriter.GetReadableStatus().ShouldEqual("cache disabled;    items: 0");

        private static InterviewHistoryWriter interviewHistoryWriter;
        private static Mock<IReadSideRepositoryWriter<InterviewSummary>> interviewSummaryWriterMock;
        private static Mock<IFileSystemAccessor> fileSystemAccessorMock;
        private static InterviewHistoryView interviewHistoryView;
        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static Guid questionnaireId = Guid.Parse("22222222222222222222222222222222");
        private static long questionnaireVersion = 2;
    }
}
