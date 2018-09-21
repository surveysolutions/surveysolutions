using NUnit.Framework;
using WB.Services.Export.CsvExport.Implementation.DoFiles;

namespace WB.Services.Export.Tests.CsvExport.Implementation.DoFiles
{
    [TestOf(typeof(QuestionnaireLabelFactory))]
    public class QuestionnaireLabelFactoryTests
    {
        [Test]
        public void should_remove_html_tags_from_question_title()
        {
            var questionnaire = Create.QuestionnaireDocument(children: 
                Create.TextQuestion(questionText: "test <strong>html</strong>"));
            var exportStructure = Create.QuestionnaireExportStructure(questionnaire);
            var factory = new QuestionnaireLabelFactory();

            // Act
            var labels = factory.CreateLabelsForQuestionnaire(exportStructure);

            // Assert
            Assert.That(labels[0].LabeledVariable[1].Label, Does.Not.Contain("<strong>").And.Not.Contain("</strong>"));
        }
    }
}
