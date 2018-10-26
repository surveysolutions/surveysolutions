using System.Collections.Generic;
using NUnit.Framework;
using WB.Services.Export.CsvExport.Implementation.DoFiles;
using WB.Services.Export.Questionnaire;

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

        [Test]
        public void should_roster_id_have_labels()
        {
            var multyOptionsQuestionId = Id.g1;

            var questionnaire = Create.QuestionnaireDocument(
                children: new IQuestionnaireEntity[]{
                    Create.MultyOptionsQuestion(id:multyOptionsQuestionId, 
                        variable:"moq",
                        options:new List<Answer>()
                        {
                            new Answer(){AnswerText = "a", AnswerValue = "1"},
                            new Answer(){AnswerText = "b", AnswerValue = "2"}
                        }),
                    Create.Roster(rosterSizeQuestionId:multyOptionsQuestionId, variable:"roster"),
                    Create.Roster(rosterSizeSourceType:RosterSizeSourceType.FixedTitles, 
                        variable:"rosterFixed",
                        fixedTitles: new FixedRosterTitle[]
                        {
                            new FixedRosterTitle(10, "aaa"),
                            new FixedRosterTitle(20, "bbb") 
                        },
                        children: new []
                        {
                            Create.NumericIntegerQuestion(Id.g3, variable:"num")
                        })
                });
            var exportStructure = Create.QuestionnaireExportStructure(questionnaire);
            var factory = new QuestionnaireLabelFactory();

            // Act
            var labels = factory.CreateLabelsForQuestionnaire(exportStructure);

            // Assert
            Assert.That(labels[1].LabeledVariable[0].VariableValueLabels[0].Label, Is.EqualTo("a"));
            Assert.That(labels[2].LabeledVariable[0].VariableValueLabels[1].Label, Is.EqualTo("bbb"));
        }
    }
}
