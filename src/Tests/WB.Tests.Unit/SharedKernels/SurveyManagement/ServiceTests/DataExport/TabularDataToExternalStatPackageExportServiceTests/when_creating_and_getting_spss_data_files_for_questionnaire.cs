﻿using System;
using System.IO;
using System.Threading;
using Machine.Specifications;
using Moq;
using StatData.Core;
using StatData.Writers;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.TabularDataToExternalStatPackageExportServiceTests
{
    internal class when_creating_and_getting_spss_data_files_for_questionnaire : TabularDataToExternalStatPackageExportServiceTestContext
    {
        Establish context = () =>
        {
            var questionnaireExportStructure = CreateQuestionnaireExportStructure(CreateHeaderStructureForLevel("main level"),
                CreateHeaderStructureForLevel("nested roster level", referenceNames: new[] { "r1", "r2" },
                    levelScopeVector: new ValueVector<Guid>(new[] { Guid.NewGuid()})));

            var fileSystemAccessor = new Mock<IFileSystemAccessor>();
            fileSystemAccessor.Setup(x => x.IsDirectoryExists(Moq.It.IsAny<string>())).Returns(true);
            fileSystemAccessor.Setup(x => x.ChangeExtension(Moq.It.IsAny<string>(), Moq.It.IsAny<string>())).Returns(fileNameExported);
            fileSystemAccessor.Setup(x => x.GetFileNameWithoutExtension(Moq.It.IsAny<string>())).Returns<string>(Path.GetFileNameWithoutExtension);

            var tabFileReader = new Mock<ITabFileReader>();
            tabFileReader.Setup(x => x.GetMetaFromTabFile(Moq.It.IsAny<string>())).Returns(meta);
            tabFileReader.Setup(x => x.GetDataFromTabFile(Moq.It.IsAny<string>())).Returns(new string[,]{{"1","1"}});

            datasetWriter = new Mock<IDatasetWriter>();

            var datasetWriterFactory = new Mock<IDatasetWriterFactory>();
            datasetWriterFactory.Setup(x => x.CreateDatasetWriter(DataExportFormat.SPSS)).Returns(datasetWriter.Object);

            _tabularDataToExternalStatPackagesTabDataExportService = CreateSqlToTabDataExportService(
                fileSystemAccessor: fileSystemAccessor.Object, questionnaireExportStructure: questionnaireExportStructure,
                tabFileReader: tabFileReader.Object, datasetWriterFactory: datasetWriterFactory.Object);
        };

        Because of = () =>
            filePaths = _tabularDataToExternalStatPackagesTabDataExportService.CreateAndGetSpssDataFilesForQuestionnaire(questionnaireId, questionnaireVersion, new[] { mailLevelFileName, nestedRosterFileName }, new Progress<int>(), CancellationToken.None);

        private It should_call_write_to_file = () =>
            datasetWriter.Verify(x => x.WriteToFile(Moq.It.IsAny<string>(), Moq.It.IsAny<IDatasetMeta>(), Moq.It.IsAny<IDataQuery>()), Times.Exactly(2));

        private static TabularDataToExternalStatPackageExportService _tabularDataToExternalStatPackagesTabDataExportService;
        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static long questionnaireVersion = 3;
        private static string[] filePaths;
        private static string mailLevelFileName = "main level.tab";
        private static string nestedRosterFileName = "nested roster level.tab";
        private static string fileNameExported = "main level.sav";
        private static Mock<IDatasetWriter> datasetWriter;

        private static DatasetMeta meta = new DatasetMeta(new IDatasetVariable[] {new DatasetVariable("a"), new DatasetVariable("ParentId1") });
    }
}
