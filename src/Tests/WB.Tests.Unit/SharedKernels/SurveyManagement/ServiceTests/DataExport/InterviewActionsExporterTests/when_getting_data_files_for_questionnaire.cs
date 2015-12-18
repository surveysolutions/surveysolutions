using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services.Exporters;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.InterviewActionsExporterTests
{
    internal class when_getting_data_files_for_questionnaire : InterviewActionsExporterTestsContext
    {
        Establish context = () =>
        {
            fileSystemAccessor = new Mock<IFileSystemAccessor>();
            fileSystemAccessor.Setup(x => x.IsDirectoryExists(Moq.It.IsAny<string>())).Returns(false);
            fileSystemAccessor.Setup(x => x.GetFilesInDirectory(Moq.It.IsAny<string>())).Returns(new[] { fileName, "2.txt" });
            fileSystemAccessor.Setup(x => x.CombinePath(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
            .Returns<string, string>(Path.Combine);

            var interviewStatuses = new TestInMemoryWriter<InterviewStatuses>();

            interviewStatuses.Store(
                Create.InterviewStatuses(questionnaireId: questionnaireId, questionnaireVersion: questionnaireVersion,
                    statuses: new[] {Create.InterviewCommentedStatus()}),
                "id");


            var questionnaireExportStructure = Create.QuestionnaireExportStructure(questionnaireId, questionnaireVersion);
            var headerStructureForLevel = Create.HeaderStructureForLevel();
            headerStructureForLevel.LevelName = "1";
            questionnaireExportStructure.HeaderToLevelMap.Add(new ValueVector<Guid>(), headerStructureForLevel);
            readSideToTabularFormatExportService = CreateExporter(csvWriter: csvWriterMock.Object,
                fileSystemAccessor: fileSystemAccessor.Object, interviewStatuses: interviewStatuses, questionnaireExportStructure: questionnaireExportStructure);
        };

        Because of = () =>
            readSideToTabularFormatExportService.ExportAll(new QuestionnaireIdentity(questionnaireId, questionnaireVersion), "", new Progress<int>());

        It should_record_one_completed_action = () =>
           csvWriterMock.Verify(x=>x.WriteData(Moq.It.IsAny<string>(),Moq.It.Is<IEnumerable<string[]>>(s=>s.Any(c=>c.Contains("Completed"))), Moq.It.IsAny<string>()), Times.Once);

        private static InterviewActionsExporter readSideToTabularFormatExportService;
        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static long questionnaireVersion = 3;
        private static Mock<IFileSystemAccessor> fileSystemAccessor;
        private static string fileName = "1.tab";
        private static Mock<ICsvWriter> csvWriterMock = new Mock<ICsvWriter>();
    }
}
 