using System;
using System.Collections.Generic;
using ddidotnet;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using StatData.Core;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.MetadataExportServiceTests
{
    internal class when_creating_and_getting_metadata_for_questionnaire : MetadataExportServiceTestContext
    {
        Establish context = () =>
        {
            var questionnaireExportStructure = CreateQuestionnaireExportStructure(CreateHeaderStructureForLevel("main level"),
                CreateHeaderStructureForLevel("nested roster level", referenceNames: new[] { "r1", "r2" },
                    levelScopeVector: new ValueVector<Guid>(new[] { Guid.NewGuid(), Guid.NewGuid() })));

            var fileSystemAccessor = new Mock<IFileSystemAccessor>();
            fileSystemAccessor.Setup(x => x.IsDirectoryExists(Moq.It.IsAny<string>())).Returns(true);
            fileSystemAccessor.Setup(x => x.GetFilesInDirectory(Moq.It.IsAny<string>())).Returns(new[] { fileName });
            fileSystemAccessor.Setup(x => x.ChangeExtension(Moq.It.IsAny<string>(), Moq.It.IsAny<string>())).Returns(fileNameExported);

            fileSystemAccessor.Setup(x => x.CombinePath(Moq.It.IsAny<string>(), Moq.It.IsAny<string>())).Returns(fileNameExported);

            var tabFileReader = new Mock<ITabFileReader>();
            tabFileReader.Setup(x => x.GetMetaFromTabFile(Moq.It.IsAny<string>())).Returns(meta);
            tabFileReader.Setup(x => x.GetDataFromTabFile(Moq.It.IsAny<string>())).Returns(new string[,]{{"1","1"}});


            var dummyMetaDescription= new MetaDescription();
            metaDescription = new Mock<IMetaDescription>();
            metaDescription.Setup(x => x.Document).Returns(dummyMetaDescription.Document);
            metaDescription.Setup(x => x.Study).Returns(dummyMetaDescription.Study);
            metaDescription.Setup(x => x.AddDataFile(Moq.It.IsAny<string>()))
                .Returns(dummyMetaDescription.AddDataFile("test"));

            var metaDescriptionFactory = new Mock<IMetaDescriptionFactory>();
            metaDescriptionFactory.Setup(x => x.CreateMetaDescription()).Returns(metaDescription.Object);

            var questionnaire = new QuestionnaireDocument()
            {
                Title = "test",

                Children = new List<IComposite>()
                {
                    new TextQuestion() { QuestionText = "test", VariableLabel = "a", Instructions = "ttt"},
                    new TextQuestion() { QuestionText = "test", VariableLabel = "r2"}

                }
            };

            var questionnaireDocumentVersionedStorage = new Mock<IReadSideKeyValueStorage<QuestionnaireDocumentVersioned>>();
            questionnaireDocumentVersionedStorage.Setup(_ => _.GetById(Moq.It.IsAny<string>()))
                .Returns(new QuestionnaireDocumentVersioned()
                {
                    Questionnaire = questionnaire
                });

            metadataExportService = CreateMetadataExportService(
                fileSystemAccessor: fileSystemAccessor.Object, questionnaireExportStructure: questionnaireExportStructure,
                tabFileReader: tabFileReader.Object,
                questionnaireDocumentVersionedStorage : questionnaireDocumentVersionedStorage.Object,
                metaDescriptionFactory: metaDescriptionFactory.Object);
        };

        Because of = () =>
            filePath = metadataExportService.CreateAndGetDDIMetadataFileForQuestionnaire(questionnaireId, questionnaireVersion, "");

        private It should_call_write_xml = () => 
            metaDescription.Verify(x => x.WriteXml(Moq.It.IsAny<string>()), Times.Once());

        private It should_return_correct_path = () =>
            filePath.ShouldEqual(fileNameExported);

        private static MetadataExportService metadataExportService;
        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static long questionnaireVersion = 3;
        private static string filePath;
        private static string fileName = "1.tab";
        private static string fileNameExported = "1.xml";
        private static Mock<IMetaDescription> metaDescription;

        private static DatasetMeta meta = new DatasetMeta(new IDatasetVariable[] {new DatasetVariable("a")});
    }
}
