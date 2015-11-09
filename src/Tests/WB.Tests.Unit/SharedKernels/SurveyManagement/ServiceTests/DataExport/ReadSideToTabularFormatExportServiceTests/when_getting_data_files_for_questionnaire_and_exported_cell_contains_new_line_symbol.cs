using System;
using System.IO;
using Machine.Specifications;
using Moq;
using Nito.AsyncEx.Synchronous;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.ReadSideToTabularFormatExportServiceTests
{
    internal class when_getting_data_files_for_questionnaire_and_exported_cell_contains_new_line_symbol : ReadSideToTabularFormatExportServiceTestContext
    {
        Establish context = () =>
        {
            fileSystemAccessor = new Mock<IFileSystemAccessor>();
            fileSystemAccessor.Setup(x => x.IsDirectoryExists(Moq.It.IsAny<string>())).Returns(false);
            fileSystemAccessor.Setup(x => x.GetFilesInDirectory(Moq.It.IsAny<string>())).Returns(new[] { fileName, "2.txt" });
            fileSystemAccessor.Setup(x => x.CombinePath(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
            .Returns<string, string>(Path.Combine);

            var interviewCommentaries = new TestInMemoryWriter<InterviewCommentaries>();

            interviewCommentaries.Store(
                Create.InterviewCommentaries(questionnaireId: questionnaireId, questionnaireVersion: questionnaireVersion,
                    comments: new[] { Create.InterviewComment(comment:Environment.NewLine+comment) }),
                "id");

            var questionnaireExportStructure = Create.QuestionnaireExportStructure(questionnaireId, questionnaireVersion);
            var headerStructureForLevel = Create.HeaderStructureForLevel();
            headerStructureForLevel.LevelName = "1";
            questionnaireExportStructure.HeaderToLevelMap.Add(new ValueVector<Guid>(), headerStructureForLevel);
            readSideToTabularFormatExportService = CreateReadSideToTabularFormatExportService(csvWriterService: csvWriterServiceMock.Object,
                fileSystemAccessor: fileSystemAccessor.Object, interviewCommentaries: interviewCommentaries, questionnaireExportStructure: questionnaireExportStructure);
        };

        Because of = () =>
            readSideToTabularFormatExportService.ExportInterviewsInTabularFormatAsync(new QuestionnaireIdentity(questionnaireId, questionnaireVersion), "").WaitAndUnwrapException();


        It should_return_correct_file_name = () =>
           fileSystemAccessor.Verify(x => x.OpenOrCreateFile(fileName, true), Times.Once);

        It should_record_one_completed_action = () =>
           csvWriterServiceMock.Verify(x => x.WriteField(comment), Times.Once);

        private static ReadSideToTabularFormatExportService readSideToTabularFormatExportService;
        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static long questionnaireVersion = 3;
        private static Mock<IFileSystemAccessor> fileSystemAccessor;
        private static string fileName = "1.tab";
        private static string comment = "test";
        private static Mock<ICsvWriterService> csvWriterServiceMock = new Mock<ICsvWriterService>();
    }
}