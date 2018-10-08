using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services;

namespace WB.Services.Export.Tests.CsvExport.Exporters
{
    internal class when_getting_data_files_for_questionnaire_and_exported_cell_contains_new_line_symbol
    {
        private static readonly string questionnaireId = "questionnaireid";
        private static readonly string fileName = "interview__comments.tab";
        private static Mock<ICsvWriter> csvWriterMock;
        private static QuestionnaireExportStructure questionnaireExportStructure;

        [Test]
        public async Task should_return_correct_file_name()
        {
            var fileSystemAccessor = new Mock<IFileSystemAccessor>();
            fileSystemAccessor.Setup(x => x.IsDirectoryExists(It.IsAny<string>())).Returns(false);
            fileSystemAccessor.Setup(x => x.GetFilesInDirectory(It.IsAny<string>())).Returns(new[] {fileName, "2.txt"});

            csvWriterMock = new Mock<ICsvWriter>();

            questionnaireExportStructure = Create.QuestionnaireExportStructure(questionnaireId);
            var headerStructureForLevel = Create.HeaderStructureForLevel();
            questionnaireExportStructure.HeaderToLevelMap.Add(new ValueVector<Guid>(), headerStructureForLevel);
            var readSideToTabularFormatExportService =
                CreateExporter(
                    fileSystemAccessor: fileSystemAccessor.Object,
                    csvWriter: csvWriterMock.Object);

            // Act
            await readSideToTabularFormatExportService.ExportAsync(questionnaireExportStructure, new List<Guid>(), "",
                Create.Tenant(), new Progress<int>(), CancellationToken.None);

            // Assert
            csvWriterMock.Verify(x => x.WriteData(fileName, It.IsAny<IEnumerable<string[]>>(), It.IsAny<string>()));
        }


        protected static CommentsExporter CreateExporter(ITenantApi<IHeadquartersApi> tenantApi = null,
            IFileSystemAccessor fileSystemAccessor = null,
            ICsvWriter csvWriter = null)
        {
            return new CommentsExporter(
                Mock.Of<IOptions<InterviewDataExportSettings>>(x => x.Value == new InterviewDataExportSettings()),
                fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>(),
                csvWriter ?? Mock.Of<ICsvWriter>(),
                tenantApi ?? Create.HeadquartersApi(),
                Mock.Of<ILogger<CommentsExporter>>());
        }
    }
}
