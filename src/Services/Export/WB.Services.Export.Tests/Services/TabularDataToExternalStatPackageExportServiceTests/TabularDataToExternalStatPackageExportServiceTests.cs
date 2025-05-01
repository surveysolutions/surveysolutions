using System;
using System.Collections.Generic;
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
using WB.Services.Export.Questionnaire.Services;
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

            var exportServiceDataProvider = new Mock<IExportServiceDataProvider>();
                exportServiceDataProvider.Setup(x => x.GetServiceDataLabels())
                .Returns(extraLabels);
            
            _tabularDataToExternalStatPackagesTabDataExportService = CreateSqlToTabDataExportService(
                questionnaireExportStructure: questionnaireExportStructure,
                tabFileReader: tabFileReader.Object, 
                datasetWriterFactory: datasetWriterFactory.Object,
                exportServiceDataProvider: exportServiceDataProvider.Object);

            // Act
            await _tabularDataToExternalStatPackagesTabDataExportService.CreateAndGetSpssDataFilesForQuestionnaireAsync(
                    Create.Tenant(), new QuestionnaireId(questionnaireId.ToString()), null, new[] { mailLevelFileName, nestedRosterFileName , extraFile },
                    new ExportProgress(), CancellationToken.None);

            //Assert
            datasetWriter.Verify(
                x => x.WriteToFile(extraFileExported, It.Is<IDatasetMeta>(meta => meta.Variables.First().VarLabel == "test label"), Moq.It.IsAny<IDataQuery>()),
                Times.Exactly(1));
        }

        [Test]
        public async Task when_creating_and_getting_spss_data_files_for_questionnaire_with_labels_for_reusable_categories()
        {
            TabularDataToExternalStatPackageExportService _tabularDataToExternalStatPackagesTabDataExportService;
            Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
            string mainLevelTab = "main_level.tab";
            string nestedRosterFileName = "nested_roster_level.tab";

            Guid category1Id = Guid.Parse("33333333333333333333333333333333");
            
            var storage = new Mock<IQuestionnaireStorage>();
            var factory = new QuestionnaireExportStructureFactory(storage.Object);
            
            QuestionnaireDocument questionnaireDocument = new QuestionnaireDocument()
            {
                PublicKey = questionnaireId,
                Title = "main_level",
                VariableName = "main_level",
                Children = new List<IQuestionnaireEntity>
                {
                    new SingleQuestion()
                    {
                        PublicKey = Guid.Parse("10000000000000000000000000000000"),
                        VariableName = "q1",
                        QuestionType = QuestionType.SingleOption,
                        CategoriesId = category1Id
                    },
                    new SingleQuestion()
                    {
                        PublicKey = Guid.Parse("20000000000000000000000000000000"),
                        VariableName = "q2",
                        QuestionType = QuestionType.SingleOption,
                        CategoriesId = category1Id
                    },
                },
                Categories =
                [
                    new Categories()
                    {
                        Id = category1Id,
                        Name = "test_categories",
                        Values =
                        [
                            new CategoryItem()
                            {
                                Id = 1,
                                ParentId = null,
                                Text = "test 1"
                            },
                            new CategoryItem()
                            {
                                Id = 2,
                                ParentId = null,
                                Text = "test 2"
                            }
                        ]

                    },
                ]
            };

            var questionnaireExportStructure1 = factory.CreateQuestionnaireExportStructure(questionnaireDocument);
            
            // var questionnaireExportStructure = CreateQuestionnaireExportStructure(questionnaireId.ToString(),
            //     CreateHeaderStructureForLevel("main level"),
            //     CreateHeaderStructureForLevel("nested roster level", referenceNames: new[] {"r1", "r2"},
            //         levelScopeVector: new ValueVector<Guid>(new[] {Guid.NewGuid()})));
            
            var tabFileReader = new Mock<ITabFileReader>();
            
            tabFileReader.Setup(x => x.GetMetaFromTabFile(Moq.It.IsAny<string>()))
                .Returns(() => new DatasetMeta([new DatasetVariable("q1"), new DatasetVariable("q2")]));
            tabFileReader.Setup(x => x.GetDataFromTabFile(Moq.It.IsAny<string>()))
                .Returns(new string[,] {{"1", "1"}});
            
            var datasetWriter = new Mock<IDatasetWriter>();
            
            var datasetWriterFactory = new Mock<IDatasetWriterFactory>();
            datasetWriterFactory.Setup(x => x.CreateDatasetWriter(DataExportFormat.SPSS)).Returns(datasetWriter.Object);
            
            //----
            
            _tabularDataToExternalStatPackagesTabDataExportService = CreateSqlToTabDataExportService(
                questionnaireExportStructure: questionnaireExportStructure1,
                tabFileReader: tabFileReader.Object, 
                datasetWriterFactory: datasetWriterFactory.Object);
            
            //act
            await _tabularDataToExternalStatPackagesTabDataExportService.CreateAndGetSpssDataFilesForQuestionnaireAsync(
                Create.Tenant(), new QuestionnaireId(questionnaireId.ToString()), null, new[] {mainLevelTab},
                new ExportProgress(), CancellationToken.None);
            
        }
    }
}
