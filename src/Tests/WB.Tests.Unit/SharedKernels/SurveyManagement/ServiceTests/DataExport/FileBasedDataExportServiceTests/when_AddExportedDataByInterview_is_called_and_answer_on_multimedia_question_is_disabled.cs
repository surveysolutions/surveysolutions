using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.FileBasedDataExportServiceTests
{
    internal class when_AddExportedDataByInterview_is_called_and_answer_on_multimedia_question_is_disabled : FileBasedDataExportServiceTestContext
    {
        Establish context = () =>
        {
            interviewExportServiceMock = new Mock<IDataExportWriter>();

            fileSystemAccessorMock = new Mock<IFileSystemAccessor>();
            fileSystemAccessorMock.Setup(x => x.IsDirectoryExists(Moq.It.IsAny<string>())).Returns(true);
            fileSystemAccessorMock.Setup(x => x.MakeValidFileName(Moq.It.IsAny<string>())).Returns("1st");
            fileSystemAccessorMock.Setup(x => x.CombinePath(Moq.It.IsAny<string>(), Moq.It.IsAny<string>())).Returns<string, string>(Path.Combine);

            interviewLevelToExport = new InterviewDataExportLevelView(new ValueVector<Guid> { Guid.NewGuid() }, "1st",
                new[]
                {
                    new InterviewDataExportRecord(interviewId, "name.tex", new string[0], new string[0],
                        new[] { new ExportedQuestion(Guid.NewGuid(), QuestionType.Multimedia, new[] { "" }), })
                }, Guid.NewGuid().FormatGuid());

            interviewToExport = new InterviewDataExportView(interviewId, Guid.NewGuid(), 1,
                new[] { interviewLevelToExport });

            plainFileRepositoryMock = new Mock<IPlainInterviewFileStorage>();
            plainFileRepositoryMock.Setup(x => x.GetInterviewBinaryData(interviewId, Moq.It.IsAny<string>()))
                .Returns(data);
            fileBasedDataExportRepositoryWriter = CreateFileBasedDataExportService(fileSystemAccessorMock.Object,
                interviewExportServiceMock.Object, plainFileRepository: plainFileRepositoryMock.Object,
                interviewDataExportView: interviewToExport);
        };

        Because of = () =>
            fileBasedDataExportRepositoryWriter.AddExportedDataByInterview(interviewId);

        It should_store_once_data_by_level = () =>
            interviewExportServiceMock.Verify(x => x.AddOrUpdateInterviewRecords(interviewToExport, Moq.It.IsAny<Guid>(), Moq.It.IsAny<long>()), Times.Once());

        It should_not_store_multimedia_file_which_is_answer_on_disabled_question = () =>
            fileSystemAccessorMock.Verify(x => x.WriteAllBytes(Moq.It.Is<string>(name => name.Contains(fileName)), data), Times.Never);

        private static FileBasedDataExportRepositoryWriter fileBasedDataExportRepositoryWriter;

        private static Mock<IDataExportWriter> interviewExportServiceMock;
        private static Mock<IPlainInterviewFileStorage> plainFileRepositoryMock;
        private static InterviewDataExportView interviewToExport;
        private static InterviewDataExportLevelView interviewLevelToExport;
        private static Guid interviewId = Guid.NewGuid();
        private static Mock<IFileSystemAccessor> fileSystemAccessorMock;
        private static string fileName = "file.jpg";
        private static byte[] data = new byte[] { 1 };
    }
}
