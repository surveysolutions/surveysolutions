using System;
using System.Collections.Generic;
using System.IO;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services.Exporters;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.CommentsExporterTests
{
    class when_getting_data_files_for_questionnaire_and_exported_cell_contains_new_line_symbol : CommentsExporterTestsContext
    {
        Establish context = () =>
        {
            var fileSystemAccessor = new Mock<IFileSystemAccessor>();
            fileSystemAccessor.Setup(x => x.IsDirectoryExists(Moq.It.IsAny<string>())).Returns(false);
            fileSystemAccessor.Setup(x => x.GetFilesInDirectory(Moq.It.IsAny<string>(), Moq.It.IsAny<bool>())).Returns(new[] { fileName, "2.txt" });
            fileSystemAccessor.Setup(x => x.CombinePath(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Returns<string, string>(Path.Combine);

            var interviewCommentaries = new TestInMemoryWriter<InterviewCommentaries>();

            interviewCommentaries.Store(
                Create.Entity.InterviewCommentaries(questionnaireId: questionnaireId, questionnaireVersion: questionnaireVersion,
                    comments: new[] { Create.Entity.InterviewComment(comment: Environment.NewLine + comment) }),
                "id");

            csvWriterMock = new Mock<ICsvWriter>();

            questionnaireExportStructure = Create.Entity.QuestionnaireExportStructure(questionnaireId, questionnaireVersion);
            var headerStructureForLevel = Create.Entity.HeaderStructureForLevel();
            headerStructureForLevel.LevelName = "1";
            questionnaireExportStructure.HeaderToLevelMap.Add(new ValueVector<Guid>(), headerStructureForLevel);
            readSideToTabularFormatExportService =
                CreateExporter(
                    fileSystemAccessor: fileSystemAccessor.Object,
                    interviewCommentaries: interviewCommentaries,
                    csvWriter: csvWriterMock.Object);
        };

        Because of = () =>
            readSideToTabularFormatExportService.Export(questionnaireExportStructure, new List<Guid>(), "", new Progress<int>());

        It should_return_correct_file_name = () =>
            csvWriterMock.Verify(x => x.WriteData(fileName, Moq.It.IsAny<IEnumerable<string[]>>(), Moq.It.IsAny<string>()));

        private static CommentsExporter readSideToTabularFormatExportService;
        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static long questionnaireVersion = 3;
        private static string fileName = "interview_comments.tab";
        private static string comment = "test";
        private static Mock<ICsvWriter> csvWriterMock;
        private static QuestionnaireExportStructure questionnaireExportStructure;
    }
}