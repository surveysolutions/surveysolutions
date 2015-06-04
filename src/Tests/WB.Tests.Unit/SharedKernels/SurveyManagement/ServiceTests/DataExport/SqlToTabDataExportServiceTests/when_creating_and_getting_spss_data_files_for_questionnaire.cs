using System;
using Machine.Specifications;
using Moq;
using StatData.Core;
using StatData.Writers;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.Services.Sql;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.SqlToTabDataExportServiceTests
{
    internal class when_creating_and_getting_spss_data_files_for_questionnaire : SqlToTabDataExportServiceTestContext
    {
        Establish context = () =>
        {
            var questionnaireExportStructure = CreateQuestionnaireExportStructure(CreateHeaderStructureForLevel("main level"),
                CreateHeaderStructureForLevel("nested roster level", referenceNames: new[] { "r1", "r2" },
                    levelScopeVector: new ValueVector<Guid>(new[] { Guid.NewGuid(), Guid.NewGuid() })));

            var sqlDataAccessor = new Mock<IExportedDataAccessor>();
            sqlDataAccessor.Setup(x => x.GetAllDataFolder(Moq.It.IsAny<string>())).Returns(string.Empty);

            var fileSystemAccessor = new Mock<IFileSystemAccessor>();
            fileSystemAccessor.Setup(x => x.IsDirectoryExists(Moq.It.IsAny<string>())).Returns(true);
            fileSystemAccessor.Setup(x => x.GetFilesInDirectory(Moq.It.IsAny<string>())).Returns(new[] { fileName, "2.txt" });
            fileSystemAccessor.Setup(x => x.ChangeExtension(Moq.It.IsAny<string>(), Moq.It.IsAny<string>())).Returns(fileNameExported);

            var tabFileReader = new Mock<ITabFileReader>();
            tabFileReader.Setup(x => x.GetMetaFromTabFile(Moq.It.IsAny<string>())).Returns(meta);
            tabFileReader.Setup(x => x.GetDataFromTabFile(Moq.It.IsAny<string>())).Returns(new string[,]{{"1","1"}});

            datasetWriter = new Mock<IDatasetWriter>();

            var datasetWriterFactory = new Mock<IDatasetWriterFactory>();
            datasetWriterFactory.Setup(x => x.CreateDatasetWriter(ExportDataType.Spss)).Returns(datasetWriter.Object);

            sqlToTabDataExportService = CreateSqlToTabDataExportService(exportedDataAccessor: sqlDataAccessor.Object,
                fileSystemAccessor: fileSystemAccessor.Object, questionnaireExportStructure: questionnaireExportStructure,
                tabFileReader: tabFileReader.Object, datasetWriterFactory: datasetWriterFactory.Object);
        };

        Because of = () =>
            filePaths = sqlToTabDataExportService.CreateAndGetSpssDataFilesForQuestionnaire(questionnaireId, questionnaireVersion, "");

        private It should_return_one_element = () =>
            filePaths.Length.ShouldEqual(1);

        private It should_return_correct_file_name = () =>
            filePaths[0].ShouldEqual(fileNameExported);

        private It should_call_write_to_file = () =>
            datasetWriter.Verify(x => x.WriteToFile(Moq.It.IsAny<string>(), Moq.It.IsAny<IDatasetMeta>(), Moq.It.IsAny<string[,]>()), Times.Once());

        private static SqlToTabDataExportService sqlToTabDataExportService;
        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static long questionnaireVersion = 3;
        private static string[] filePaths;
        private static string fileName = "1.tab";
        private static string fileNameExported = "1.sav";
        private static Mock<IDatasetWriter> datasetWriter;

        private static DatasetMeta meta = new DatasetMeta(new IDatasetVariable[] {new DatasetVariable("a")});
    }
}
