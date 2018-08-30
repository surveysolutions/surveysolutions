using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Moq;
using NUnit.Framework;
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
    internal class TabularDataToExternalStatPackageExportServiceTests : TabularDataToExternalStatPackageExportServiceTestContext
    {
        [Test]
        public void when_creating_and_getting_spss_data_files_for_questionnaire()
        {
            TabularDataToExternalStatPackageExportService _tabularDataToExternalStatPackagesTabDataExportService;
            Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
            long questionnaireVersion = 3;
            string[] filePaths;
            string mailLevelFileName = "main level.tab";
            string nestedRosterFileName = "nested roster level.tab";
            string fileNameExported = "main level.sav";
            Mock<IDatasetWriter> datasetWriter;

            
            var questionnaireExportStructure = CreateQuestionnaireExportStructure(
                CreateHeaderStructureForLevel("main level"),
                CreateHeaderStructureForLevel("nested roster level", referenceNames: new[] {"r1", "r2"},
                    levelScopeVector: new ValueVector<Guid>(new[] {Guid.NewGuid()})));

            var fileSystemAccessor = new Mock<IFileSystemAccessor>();
            fileSystemAccessor.Setup(x => x.IsDirectoryExists(Moq.It.IsAny<string>())).Returns(true);
            fileSystemAccessor.Setup(x => x.ChangeExtension(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Returns(fileNameExported);
            fileSystemAccessor.Setup(x => x.GetFileNameWithoutExtension(Moq.It.IsAny<string>()))
                .Returns<string>(Path.GetFileNameWithoutExtension);

            var tabFileReader = new Mock<ITabFileReader>();
            tabFileReader.Setup(x => x.GetMetaFromTabFile(Moq.It.IsAny<string>())).Returns(()=>new DatasetMeta(new IDatasetVariable[] { new DatasetVariable("a"), new DatasetVariable("ParentId1") }));
            tabFileReader.Setup(x => x.GetDataFromTabFile(Moq.It.IsAny<string>())).Returns(new string[,] {{"1", "1"}});

            datasetWriter = new Mock<IDatasetWriter>();

            var datasetWriterFactory = new Mock<IDatasetWriterFactory>();
            datasetWriterFactory.Setup(x => x.CreateDatasetWriter(DataExportFormat.SPSS)).Returns(datasetWriter.Object);

            _tabularDataToExternalStatPackagesTabDataExportService = CreateSqlToTabDataExportService(
                fileSystemAccessor: fileSystemAccessor.Object,
                questionnaireExportStructure: questionnaireExportStructure,
                tabFileReader: tabFileReader.Object, datasetWriterFactory: datasetWriterFactory.Object);
            
            //
            filePaths =
                _tabularDataToExternalStatPackagesTabDataExportService.CreateAndGetSpssDataFilesForQuestionnaire(
                    questionnaireId, questionnaireVersion, new[] {mailLevelFileName, nestedRosterFileName},
                    new Progress<int>(), CancellationToken.None);

           //Assert
            datasetWriter.Verify(
                x => x.WriteToFile(Moq.It.IsAny<string>(), Moq.It.IsAny<IDatasetMeta>(), Moq.It.IsAny<IDataQuery>()),
                Times.Exactly(2));
        }

        [Test]
        public void when_creating_and_getting_spss_data_files_for_questionnaire_with_labels_for_extra_files()
        {
            TabularDataToExternalStatPackageExportService _tabularDataToExternalStatPackagesTabDataExportService;
            Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
            long questionnaireVersion = 3;
            string[] filePaths;
            string mailLevelFileName = "main level.tab";
            string nestedRosterFileName = "nested roster level.tab";
            string fileNameExported = "main level.sav";
            string extraFile = "extra.tab";
            string extraFileExported = "extra.sav";

            Mock<IDatasetWriter> datasetWriter;


            var questionnaireExportStructure = CreateQuestionnaireExportStructure(
                CreateHeaderStructureForLevel("main level"),
                CreateHeaderStructureForLevel("nested roster level", referenceNames: new[] {"r1", "r2"},
                    levelScopeVector: new ValueVector<Guid>(new[] {Guid.NewGuid()})));

            var fileSystemAccessor = new Mock<IFileSystemAccessor>();
            fileSystemAccessor.Setup(x => x.IsDirectoryExists(Moq.It.IsAny<string>())).Returns(true);
            fileSystemAccessor.Setup(x => x.ChangeExtension(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Returns((string path, string extension) => Path.ChangeExtension(path, extension));
                
            fileSystemAccessor.Setup(x => x.GetFileNameWithoutExtension(Moq.It.IsAny<string>()))
                .Returns<string>(Path.GetFileNameWithoutExtension);

            var tabFileReader = new Mock<ITabFileReader>();

            tabFileReader.Setup(x => x.GetMetaFromTabFile(Moq.It.IsAny<string>())).Returns(() =>
                new DatasetMeta(new IDatasetVariable[] {new DatasetVariable("test")}));
            tabFileReader.Setup(x => x.GetDataFromTabFile(Moq.It.IsAny<string>())).Returns(new string[,] {{"1", "1"}});

            datasetWriter = new Mock<IDatasetWriter>();

            var datasetWriterFactory = new Mock<IDatasetWriterFactory>();
            datasetWriterFactory.Setup(x => x.CreateDatasetWriter(DataExportFormat.SPSS)).Returns(datasetWriter.Object);

            var extraLabels = new Dictionary<string, Dictionary<string, string>>
            {
                {"extra", new Dictionary<string, string>() {{"test", "test label"}}}
            };

            var exportSeviceDataProvider = new Mock<IExportServiceDataProvider>();
                exportSeviceDataProvider.Setup(x => x.GetServiceDataLabels())
                .Returns(extraLabels);
            
            _tabularDataToExternalStatPackagesTabDataExportService = CreateSqlToTabDataExportService(
                fileSystemAccessor: fileSystemAccessor.Object,
                questionnaireExportStructure: questionnaireExportStructure,
                tabFileReader: tabFileReader.Object, 
                datasetWriterFactory: datasetWriterFactory.Object,
                exportServiceDataProvider: exportSeviceDataProvider.Object);

            //
            filePaths =
                _tabularDataToExternalStatPackagesTabDataExportService.CreateAndGetSpssDataFilesForQuestionnaire(
                    questionnaireId, questionnaireVersion, new[] { mailLevelFileName, nestedRosterFileName , extraFile },
                    new Progress<int>(), CancellationToken.None);

            //Assert
            datasetWriter.Verify(
                x => x.WriteToFile(extraFileExported, It.Is<IDatasetMeta>(meta => meta.Variables.First().VarLabel == "test label"), Moq.It.IsAny<IDataQuery>()),
                Times.Exactly(1));
        }
    }
}
