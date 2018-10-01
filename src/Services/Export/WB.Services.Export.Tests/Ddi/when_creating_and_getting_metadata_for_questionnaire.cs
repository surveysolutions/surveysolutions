using System;
using System.IO;
using System.Threading.Tasks;
using ddidotnet;
using FluentAssertions;
using Moq;
using WB.Services.Export.CsvExport.Implementation.DoFiles;
using WB.Services.Export.Ddi;
using WB.Services.Export.Ddi.Implementation;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Tests.Ddi
{
    internal class when_creating_and_getting_metadata_for_questionnaire : MetadataExportServiceTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public async Task context () {
            var questionnaireLabelFactoryMock = new Mock<IQuestionnaireLabelFactory>();
            var textQuestionId = Guid.NewGuid();
            var numericQuestionId = Guid.NewGuid();
            var singleOptionQuestionId = Guid.NewGuid();
            var gpsQuestionId = Guid.NewGuid();

            var questionnaire = Create.QuestionnaireDocument(children: new IQuestionnaireEntity[]
            {
                Create.TextQuestion(id: textQuestionId, questionText: "text question", variableLabel: "a", instructions: "ttt"),
                Create.NumericRealQuestion(id: numericQuestionId, questionText: "numeric question"),
                Create.SingleOptionQuestion(id: singleOptionQuestionId, questionText: "single option question"),
                Create.GpsCoordinateQuestion(questionId: gpsQuestionId, title: "gps question")
            });

            questionnaire.Title = "main level";

            questionnaireLabelFactoryMock.Setup(
                x => x.CreateLabelsForQuestionnaire(Moq.It.IsAny<QuestionnaireExportStructure>()))
                .Returns(new[]
                {
                    Create.QuestionnaireLevelLabels("main level",
                        Create.LabeledVariable(variableName: "txt", label: "lbl_txt", questionId: textQuestionId),
                        Create.LabeledVariable(variableName: "num", questionId: numericQuestionId),
                        Create.LabeledVariable(variableName: "sng", questionId: singleOptionQuestionId,
                            variableValueLabels:
                                new[]
                                {
                                    Create.VariableValueLabel(value: "1", label: "t1"),
                                    Create.VariableValueLabel(value: "2", label: "t2")
                                }),
                        Create.LabeledVariable(variableName: "gps", questionId: gpsQuestionId)),
                    Create.QuestionnaireLevelLabels("nested roster level",
                        Create.LabeledVariable(variableName: "r1"), Create.LabeledVariable(variableName: "r2"))
                });

            var fileSystemAccessor = new Mock<IFileSystemAccessor>();
            fileSystemAccessor.Setup(x => x.CombinePath(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Returns<string, string>(Path.Combine);

            metadataWriter = new Mock<IMetadataWriter>();
            var metaDescriptionFactory = new Mock<IMetaDescriptionFactory>();

            metaDescriptionFactory.Setup(x => x.CreateMetaDescription()).Returns(metadataWriter.Object);

            ddiMetadataFactory = CreateMetadataExportService(
                questionnaireDocument: questionnaire,
                metaDescriptionFactory: metaDescriptionFactory.Object,
                questionnaireLabelFactory: questionnaireLabelFactoryMock.Object);
            await BecauseOf();
        }

        private async Task BecauseOf()
        {
            filePath = await 
                ddiMetadataFactory.CreateDDIMetadataFileForQuestionnaireInFolder(Create.Tenant(), new QuestionnaireId(questionnaireId), "");
        }

        [NUnit.Framework.Test] public void should_call_write_xml () =>
            metadataWriter.Verify(x => x.SaveMetadataInFile(Moq.It.IsAny<string>()), Times.Once());

        [NUnit.Framework.Test] public void should_add_one_file_with_questionnaire_name () =>
            metadataWriter.Verify(x => x.CreateDdiDataFile("main level"), Times.Once());

        [NUnit.Framework.Test] public void should_return_correct_path () =>
            filePath.Should().Be("11111111-1111-1111-1111-111111111111_3_ddi.xml");

        [NUnit.Framework.Test] public void should_add_variable_for_txt_question () =>
            metadataWriter.Verify(x =>x.AddDdiVariableToFile(Moq.It.IsAny<DdiDataFile>(), "txt", DdiDataType.DynString, "lbl_txt", "ttt","text question", null));

        [NUnit.Framework.Test] public void should_add_variable_for_num_question () =>
            metadataWriter.Verify(x =>x.AddDdiVariableToFile(Moq.It.IsAny<DdiDataFile>(), "num", DdiDataType.Numeric, "lbl", null,"numeric question", DdiVariableScale.Scale));

        [NUnit.Framework.Test] public void should_add_variable_for_sng_question () =>
            metadataWriter.Verify(x =>x.AddDdiVariableToFile(Moq.It.IsAny<DdiDataFile>(), "sng", DdiDataType.Numeric, "lbl", null,"single option question", DdiVariableScale.Nominal));

        [NUnit.Framework.Test] public void should_add_value_label_t1_for_question_sng () =>
            metadataWriter.Verify(x => x.AddValueLabelToVariable(Moq.It.IsAny<DdiVariable>(), 1, "t1"));

        [NUnit.Framework.Test] public void should_add_value_label_t2_for_question_sng () =>
            metadataWriter.Verify(x => x.AddValueLabelToVariable(Moq.It.IsAny<DdiVariable>(), 2, "t2"));

        [NUnit.Framework.Test] public void should_add_variable_for_gps_question () =>
            metadataWriter.Verify(x =>x.AddDdiVariableToFile(Moq.It.IsAny<DdiDataFile>(), "gps", DdiDataType.Numeric, "lbl", null,"gps question", DdiVariableScale.Scale));

        [NUnit.Framework.Test] public void should_add_variable_r1 () =>
            metadataWriter.Verify(x =>x.AddDdiVariableToFile(Moq.It.IsAny<DdiDataFile>(), "r1", DdiDataType.DynString, "lbl", null, null,null));

        [NUnit.Framework.Test] public void should_add_variable_r2 () =>
            metadataWriter.Verify(x =>x.AddDdiVariableToFile(Moq.It.IsAny<DdiDataFile>(), "r1", DdiDataType.DynString, "lbl", null, null,null));

        private static DdiMetadataFactory ddiMetadataFactory;
        private static string questionnaireId = "11111111111111111111111111111111";
        private static long questionnaireVersion = 3;
        private static string filePath;
        private static Mock<IMetadataWriter> metadataWriter;
    }
}
