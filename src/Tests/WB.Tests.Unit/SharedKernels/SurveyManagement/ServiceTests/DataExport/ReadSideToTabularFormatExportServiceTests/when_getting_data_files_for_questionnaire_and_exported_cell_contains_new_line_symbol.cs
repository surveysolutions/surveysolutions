using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.ReadSideToTabularFormatExportServiceTests
{
    internal class when_getting_data_files_for_questionnaire_and_exported_cell_contains_new_line_symbol : ReadSideToTabularFormatExportServiceTestContext
    {
        Establish context = () =>
        {
            var fileSystemAccessor = new Mock<IFileSystemAccessor>();
            fileSystemAccessor.Setup(x => x.IsDirectoryExists(Moq.It.IsAny<string>())).Returns(false);
            fileSystemAccessor.Setup(x => x.GetFilesInDirectory(Moq.It.IsAny<string>())).Returns(new[] { fileName, "2.txt" });
            fileSystemAccessor.Setup(x => x.CombinePath(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
            .Returns<string, string>(Path.Combine);

            var interviewCommentaries = new TestInMemoryWriter<InterviewCommentaries>();

            interviewCommentaries.Store(
                Create.InterviewCommentaries(questionnaireId: questionnaireId, questionnaireVersion: questionnaireVersion,
                    comments: new[] { Create.InterviewComment(comment:Environment.NewLine+comment) }),
                "id");

            csvWriterMock = new Mock<ICsvWriter>();

            var questionnaireExportStructure = Create.QuestionnaireExportStructure(questionnaireId, questionnaireVersion);
            var headerStructureForLevel = Create.HeaderStructureForLevel();
            headerStructureForLevel.LevelName = "1";
            questionnaireExportStructure.HeaderToLevelMap.Add(new ValueVector<Guid>(), headerStructureForLevel);
            readSideToTabularFormatExportService = 
                CreateReadSideToTabularFormatExportService(
                    fileSystemAccessor: fileSystemAccessor.Object, 
                    interviewCommentaries: interviewCommentaries, 
                    questionnaireExportStructure: questionnaireExportStructure,
                    csvWriter: csvWriterMock.Object);
        };

        Because of = () =>
            readSideToTabularFormatExportService.ExportInterviewsInTabularFormat(new QuestionnaireIdentity(questionnaireId, questionnaireVersion), "", new Microsoft.Progress<int>(), CancellationToken.None);

        It should_return_correct_file_name = () =>
            csvWriterMock.Verify(x => x.WriteData(fileName, Moq.It.IsAny<IEnumerable<string[]>>(), Moq.It.IsAny<string>()));

        private static ReadSideToTabularFormatExportService readSideToTabularFormatExportService;
        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static long questionnaireVersion = 3;
        private static string fileName = "interview_comments.tab";
        private static string comment = "test";
        private static Mock<ICsvWriter> csvWriterMock;
    }
}