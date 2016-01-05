using System;
using System.Collections.Generic;
using ddidotnet;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
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
using System.IO;
using System.Reflection;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.MetadataExportServiceTests
{
    internal class when_creating_and_getting_metadata_for_questionnaire : MetadataExportServiceTestContext
    {
        Establish context = () =>
        {
            var textQuestionId = Guid.NewGuid();
            var numericQuestionId = Guid.NewGuid();
            var singleOptionQuestionId = Guid.NewGuid();
            var gpsQuestionId = Guid.NewGuid();

            var questionnaire = Create.QuestionnaireDocument(children: new IComposite[]
            {
                Create.TextQuestion(questionId:textQuestionId, text: "text question", label: "a", instruction: "ttt"),
                Create.NumericQuestion(questionId:numericQuestionId, title: "numeric question"),
                Create.SingleOptionQuestion(questionId:singleOptionQuestionId, title: "single option question"),
                Create.GpsCoordinateQuestion(questionId:gpsQuestionId, title: "gps question")
            });

            questionnaire.Title = "test";

            var mailLevelStructure = CreateHeaderStructureForLevel("main level");
            mailLevelStructure.HeaderItems =
                new Dictionary<Guid, ExportedHeaderItem>
                {
                    {
                        textQuestionId,
                        CreateExportedHeaderItem(questionId: textQuestionId, columnNames: new[] {"txt"})
                    },
                    {
                        numericQuestionId,
                        CreateExportedHeaderItem(questionId: numericQuestionId, type: QuestionType.Numeric,
                            columnNames: new[] {"num"})
                    },
                    {
                        singleOptionQuestionId,
                        CreateExportedHeaderItem(questionId: singleOptionQuestionId, type: QuestionType.SingleOption,
                            columnNames: new[] {"sng"},
                            labels: new[] {CreateLabelItem("1", "t1"), CreateLabelItem("2", "t2")})
                    },
                    {
                        gpsQuestionId,
                        CreateExportedHeaderItem(questionId: gpsQuestionId, type: QuestionType.GpsCoordinates,
                            columnNames: new[] {"gps"})
                    }
                };

            var nestedRosterLevelStructure = CreateHeaderStructureForLevel("nested roster level", referenceNames: new[] { "r1", "r2" },
                    levelScopeVector: new ValueVector<Guid>(new[] { Guid.NewGuid() }));

            nestedRosterLevelStructure.HeaderItems =
              new Dictionary<Guid, ExportedHeaderItem>();

            var questionnaireExportStructure = CreateQuestionnaireExportStructure(
                mailLevelStructure, nestedRosterLevelStructure);

            var fileSystemAccessor = new Mock<IFileSystemAccessor>();
            fileSystemAccessor.Setup(x => x.CombinePath(Moq.It.IsAny<string>(), Moq.It.IsAny<string>())).Returns<string, string>(Path.Combine);

            var tabFileReader = new Mock<ITabFileReader>();
            tabFileReader.Setup(x => x.GetMetaFromTabFile(Moq.It.IsAny<string>())).Returns(meta);
            tabFileReader.Setup(x => x.GetDataFromTabFile(Moq.It.IsAny<string>())).Returns(new string[,]{{"1","1"}});
            
            metaDescription = new Mock<IMetaDescription>();
            var dummyMetaDescription = new MetaDescription();

            ddiMainLevelDataFile = dummyMetaDescription.AddDataFile("main level");
            ddiNestedRosterDataFile = dummyMetaDescription.AddDataFile("nested roster level");
            metaDescription.Setup(x => x.Document).Returns(dummyMetaDescription.Document);
            metaDescription.Setup(x => x.Study).Returns(dummyMetaDescription.Study);

            metaDescription.Setup(x => x.AddDataFile("main level")).Returns(ddiMainLevelDataFile);
            metaDescription.Setup(x => x.AddDataFile("nested roster level")).Returns(ddiNestedRosterDataFile);

            var metaDescriptionFactory = new Mock<IMetaDescriptionFactory>();
            metaDescriptionFactory.Setup(x => x.CreateMetaDescription()).Returns(metaDescription.Object);

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
        {
            filePath =
                metadataExportService.CreateAndGetDDIMetadataFileForQuestionnaire(questionnaireId,
                    questionnaireVersion, "");
            mainLevelVariables = GetDdiFileVariables(ddiMainLevelDataFile);
            nestedRosterVariables= GetDdiFileVariables(ddiNestedRosterDataFile);
        };

        It should_call_write_xml = () => 
            metaDescription.Verify(x => x.WriteXml(Moq.It.IsAny<string>()), Times.Once());

        It should_add_one_file_with_questionnaire_name = () =>
            metaDescription.Verify(x => x.AddDataFile("main level"), Times.Once());

        It should_return_correct_path = () =>
            filePath.ShouldEqual("11111111-1111-1111-1111-111111111111_3_ddi.xml");

        It should_add_id_as_first_variable_of_main_level = () =>
            mainLevelVariables[0].Name.ShouldEqual("Id");

        It should_add_txt_as_second_variable_of_main_level = () =>
            mainLevelVariables[1].Name.ShouldEqual("txt");

        It should_add_instructions_for_txt_variable_of_main_level = () =>
            mainLevelVariables[1].IvuInstr.ShouldEqual("ttt");

        It should_add_question_literal_for_txt_variable_of_main_level = () =>
            mainLevelVariables[1].QstnLit.ShouldEqual("text question");

        It should_add_num_as_third_variable_of_main_level = () =>
            mainLevelVariables[2].Name.ShouldEqual("num");

        It should_add_sng_as_fourth_variable_of_main_level = () =>
            mainLevelVariables[3].Name.ShouldEqual("sng");

        It should_add_value_label_t1_for_question_sng = () =>
           GetValueLabels(mainLevelVariables[3])[1].ShouldEqual("t1");

        It should_add_value_label_t2_for_question_sng = () =>
           GetValueLabels(mainLevelVariables[3])[2].ShouldEqual("t2");

        It should_add_gps_as_fifth_variable_of_main_level = () =>
            mainLevelVariables[4].Name.ShouldEqual("gps");

        It should_add_id_as_first_variable_of_nested_roster_level = () =>
           nestedRosterVariables[0].Name.ShouldEqual("Id");

        It should_add_r1_as_second_variable_of_nested_roster_level = () =>
            nestedRosterVariables[1].Name.ShouldEqual("r1");

        It should_add_r2_as_third_variable_of_nested_roster_level = () =>
            nestedRosterVariables[2].Name.ShouldEqual("r2");

        It should_add_ParentId1_as_fourth_variable_of_nested_roster_level = () =>
            nestedRosterVariables[3].Name.ShouldEqual("ParentId1");

        private static List<DdiVariable> GetDdiFileVariables(DdiDataFile ddiDataFile)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            FieldInfo field = typeof(DdiDataFile).GetField("Variables", bindFlags);
            return field.GetValue(ddiDataFile) as List<DdiVariable>;
        }

        private static Dictionary<decimal,string> GetValueLabels(DdiVariable ddiVariable)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            FieldInfo field = typeof(DdiVariable).GetField("_valueLabels", bindFlags);
            return field.GetValue(ddiVariable) as Dictionary<decimal, string>;
        }

        private static MetadataExportService metadataExportService;
        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static long questionnaireVersion = 3;
        private static string filePath;
        private static Mock<IMetaDescription> metaDescription;
        private static List<DdiVariable> mainLevelVariables;
        private static List<DdiVariable> nestedRosterVariables;
        private static DdiDataFile ddiMainLevelDataFile;
        private static DdiDataFile ddiNestedRosterDataFile;
        private static DatasetMeta meta = new DatasetMeta(new IDatasetVariable[] {new DatasetVariable("a")});
    }
}
