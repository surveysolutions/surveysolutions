using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.Factories
{
    [TestOf(typeof(QuestionnaireLabelFactory))]
    public class QuestionnaireLabelFactoryTests
    {
        [Test]
        public void should_remove_html_tags_from_question_title()
        {
            var questionnaire = Create.Entity.QuestionnaireDocument(children: 
                Create.Entity.TextListQuestion(questionText: "test <strong>html</strong>", maxAnswerCount: 25));
            var exportStructure = Create.Entity.QuestionnaireExportStructure(questionnaire);
            var factory = new QuestionnaireLabelFactory();

            // Act
            var labels = factory.CreateLabelsForQuestionnaire(exportStructure);

            // Assert
            Assert.That(labels[0].LabeledVariable[1].Label, Does.Not.Contain("<strong>").And.Not.Contain("</strong>"));
        }
    }
}