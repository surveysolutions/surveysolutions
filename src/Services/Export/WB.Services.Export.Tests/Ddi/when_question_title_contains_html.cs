using Moq;
using NUnit.Framework;
using WB.Services.Export.CsvExport.Implementation.DoFiles;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Tests.Ddi
{
    internal class when_question_title_contains_html : MetadataExportServiceTestContext
    {
        [Test]
        public void should_cut_html_tags_from_label()
        {
            var textQuestionId = Id.g1;
            var questionnaire = Create.QuestionnaireDocument(children: new IQuestionnaireEntity[]
            {
                Create.TextQuestion(id: textQuestionId,
                    questionText: "text <strong>question</start>",
                    variableLabel: "a",
                    instructions: "ttt")
            });

            questionnaire.Title = "main";
            var questionnaireLabelFactoryMock = new Mock<IQuestionnaireLabelFactory>();
            questionnaireLabelFactoryMock.Setup(
                    x => x.CreateLabelsForQuestionnaire(Moq.It.IsAny<QuestionnaireExportStructure>()))
                .Returns(new[]
                {
                    Create.QuestionnaireLevelLabels("main level",
                        Create.LabeledVariable(variableName: "txt", questionId: textQuestionId))
                });
        }
    }
}
