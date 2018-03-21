using System;
using System.IO;
using System.Threading;
using FluentAssertions;
using Moq;
using StatData.Core;
using StatData.Writers;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.ValueObjects;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.TabularDataToExternalStatPackageExportServiceTests
{
    internal class when_creating_and_getting_stata_data_files_for_questionnaire : TabularDataToExternalStatPackageExportServiceTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var questionnaireExportStructure = CreateQuestionnaireExportStructure(CreateHeaderStructureForLevel("main level"),
                CreateHeaderStructureForLevel("nested roster level", referenceNames: new[] { "r1", "r2" },
                    levelScopeVector: new ValueVector<Guid>(new[] { Guid.NewGuid(), Guid.NewGuid() })));

            var fileSystemAccessor = new Mock<IFileSystemAccessor>();
            fileSystemAccessor.Setup(x => x.IsDirectoryExists(Moq.It.IsAny<string>())).Returns(true);
            fileSystemAccessor.Setup(x => x.ChangeExtension(Moq.It.IsAny<string>(), Moq.It.IsAny<string>())).Returns(fileNameExported);
            fileSystemAccessor.Setup(x => x.GetFileNameWithoutExtension(Moq.It.IsAny<string>())).Returns<string>(Path.GetFileNameWithoutExtension);

            var tabFileReader = new Mock<ITabFileReader>();
            tabFileReader.Setup(x => x.GetMetaFromTabFile(Moq.It.IsAny<string>())).Returns(meta);
            tabFileReader.Setup(x => x.GetDataFromTabFile(Moq.It.IsAny<string>())).Returns(new string[,]{{"1","1"}});

            datasetWriter = new Mock<IDatasetWriter>();

            var datasetWriterFactory = new Mock<IDatasetWriterFactory>();
            datasetWriterFactory.Setup(x => x.CreateDatasetWriter(DataExportFormat.STATA)).Returns(datasetWriter.Object);

            _tabularDataToExternalStatPackagesTabDataExportService = CreateSqlToTabDataExportService(
                fileSystemAccessor: fileSystemAccessor.Object, questionnaireExportStructure: questionnaireExportStructure,
                tabFileReader: tabFileReader.Object, datasetWriterFactory: datasetWriterFactory.Object);
            BecauseOf();
        }

        public void BecauseOf() =>
            filePaths = _tabularDataToExternalStatPackagesTabDataExportService.CreateAndGetStataDataFilesForQuestionnaire(questionnaireId, questionnaireVersion, new[] { fileName }, new Progress<int>(), CancellationToken.None);

        [NUnit.Framework.Test] public void should_call_write_to_file () =>
            datasetWriter.Verify(x => x.WriteToFile(Moq.It.IsAny<string>(), Moq.It.IsAny<IDatasetMeta>(), Moq.It.IsAny<IDataQuery>()), Times.Once());

        private static TabularDataToExternalStatPackageExportService _tabularDataToExternalStatPackagesTabDataExportService;
        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static long questionnaireVersion = 3;
        private static string[] filePaths;
        private static string fileName = "main level.tab";
        private static string fileNameExported = "main level.dat";
        private static Mock<IDatasetWriter> datasetWriter;

        private static DatasetMeta meta = new DatasetMeta(new IDatasetVariable[] {new DatasetVariable("a")});
    }
}
