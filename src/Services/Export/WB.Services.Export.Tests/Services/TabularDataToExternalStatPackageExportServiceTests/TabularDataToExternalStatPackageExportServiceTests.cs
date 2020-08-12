﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using StatData.Core;
using StatData.Writers;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services;
using WB.Services.Export.Services.Processing;

namespace WB.Services.Export.Tests.Services.TabularDataToExternalStatPackageExportServiceTests
{
    internal class TabularDataToExternalStatPackageExportServiceTests : TabularDataToExternalStatPackageExportServiceTestContext
    {
        [Test]
        public async Task when_creating_and_getting_spss_data_files_for_questionnaire()
        {
            TabularDataToExternalStatPackageExportService _tabularDataToExternalStatPackagesTabDataExportService;
            Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
            string mailLevelFileName = "main level.tab";
            string nestedRosterFileName = "nested roster level.tab";

            var questionnaireExportStructure = CreateQuestionnaireExportStructure(questionnaireId.ToString(),
                CreateHeaderStructureForLevel("main level"),
                CreateHeaderStructureForLevel("nested roster level", referenceNames: new[] {"r1", "r2"},
                    levelScopeVector: new ValueVector<Guid>(new[] {Guid.NewGuid()})));

            var fileSystemAccessor = new Mock<IFileSystemAccessor>();
            fileSystemAccessor.Setup(x => x.IsDirectoryExists(Moq.It.IsAny<string>())).Returns(true);
            
            var tabFileReader = new Mock<ITabFileReader>();
            tabFileReader.Setup(x => x.GetMetaFromTabFile(Moq.It.IsAny<string>())).Returns(()=>new DatasetMeta(new IDatasetVariable[] { new DatasetVariable("a"), new DatasetVariable("ParentId1") }));
            tabFileReader.Setup(x => x.GetDataFromTabFile(Moq.It.IsAny<string>())).Returns(new string[,] {{"1", "1"}});

            var datasetWriter = new Mock<IDatasetWriter>();

            var datasetWriterFactory = new Mock<IDatasetWriterFactory>();
            datasetWriterFactory.Setup(x => x.CreateDatasetWriter(DataExportFormat.SPSS)).Returns(datasetWriter.Object);

            _tabularDataToExternalStatPackagesTabDataExportService = CreateSqlToTabDataExportService(
                questionnaireExportStructure: questionnaireExportStructure,
                tabFileReader: tabFileReader.Object, 
                datasetWriterFactory: datasetWriterFactory.Object);
            
            //act
            await _tabularDataToExternalStatPackagesTabDataExportService.CreateAndGetSpssDataFilesForQuestionnaireAsync(
                    Create.Tenant(), new QuestionnaireId(questionnaireId.ToString()), null, new[] {mailLevelFileName, nestedRosterFileName},
                    new ExportProgress(), CancellationToken.None);

           //Assert
            datasetWriter.Verify(
                x => x.WriteToFile(Moq.It.IsAny<string>(), Moq.It.IsAny<IDatasetMeta>(), Moq.It.IsAny<IDataQuery>()),
                Times.Exactly(2));
        }

        [Test]
        public async Task when_creating_and_getting_spss_data_files_for_questionnaire_with_labels_for_extra_files()
        {
            TabularDataToExternalStatPackageExportService _tabularDataToExternalStatPackagesTabDataExportService;
            Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
            string mailLevelFileName = "main level.tab";
            string nestedRosterFileName = "nested roster level.tab";
            string extraFile = "extra.tab";
            string extraFileExported = "extra.sav";

            var questionnaireExportStructure = CreateQuestionnaireExportStructure(questionnaireId.ToString(),
                CreateHeaderStructureForLevel("main level"),
                CreateHeaderStructureForLevel("nested roster level", referenceNames: new[] {"r1", "r2"},
                    levelScopeVector: new ValueVector<Guid>(new[] {Guid.NewGuid()})));

            var tabFileReader = new Mock<ITabFileReader>();

            tabFileReader.Setup(x => x.GetMetaFromTabFile(Moq.It.IsAny<string>())).Returns(() =>
                new DatasetMeta(new IDatasetVariable[] {new DatasetVariable("test")}));
            tabFileReader.Setup(x => x.GetDataFromTabFile(Moq.It.IsAny<string>())).Returns(new string[,] {{"1", "1"}});

            var datasetWriter = new Mock<IDatasetWriter>();

            var datasetWriterFactory = new Mock<IDatasetWriterFactory>();
            datasetWriterFactory.Setup(x => x.CreateDatasetWriter(DataExportFormat.SPSS)).Returns(datasetWriter.Object);

            var extraLabels = new Dictionary<string, Dictionary<string, HeaderItemDescription>>
            {
                {"extra", new Dictionary<string, HeaderItemDescription>() {{"test", new HeaderItemDescription("test label", ExportValueType.String)}}}
            };

            var exportSeviceDataProvider = new Mock<IExportServiceDataProvider>();
                exportSeviceDataProvider.Setup(x => x.GetServiceDataLabels())
                .Returns(extraLabels);
            
            _tabularDataToExternalStatPackagesTabDataExportService = CreateSqlToTabDataExportService(
                questionnaireExportStructure: questionnaireExportStructure,
                tabFileReader: tabFileReader.Object, 
                datasetWriterFactory: datasetWriterFactory.Object,
                exportServiceDataProvider: exportSeviceDataProvider.Object);

            // Act
            await _tabularDataToExternalStatPackagesTabDataExportService.CreateAndGetSpssDataFilesForQuestionnaireAsync(
                    Create.Tenant(), new QuestionnaireId(questionnaireId.ToString()), null, new[] { mailLevelFileName, nestedRosterFileName , extraFile },
                    new ExportProgress(), CancellationToken.None);

            //Assert
            datasetWriter.Verify(
                x => x.WriteToFile(extraFileExported, It.Is<IDatasetMeta>(meta => meta.Variables.First().VarLabel == "test label"), Moq.It.IsAny<IDataQuery>()),
                Times.Exactly(1));
        }
    }
}
