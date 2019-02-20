using System;
using System.Linq;
using NUnit.Framework;
using WB.Services.Export.InterviewDataStorage.InterviewDataExport;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Tests.InterviewDataExport
{
    [TestOf(typeof(QuestionnaireDatabaseStructure))]
    public class QuestionnaireDatabaseStructureTests
    {
        [Test]
        public void when_questionnaire_has_a_lot_of_entities_on_one_level_then_should_split_it_on_tables()
        {
            var questionnaireId = Guid.NewGuid();
            var questionnaire = Create.QuestionnaireDocument(questionnaireId, variableName: "test_q", 
                children: Enumerable.Range(0, 2000)
                    .Select(index => Create.Group(Guid.NewGuid(), "group" + index, 
                        children: Create.TextQuestion(Guid.NewGuid(), variableLabel: "t" + index)))
                    .Cast<IQuestionnaireEntity>()
                    .ToArray());
            questionnaire.ConnectChildrenWithParent();

            var structure = new QuestionnaireDatabaseStructure(questionnaire);

            Assert.That(structure.GetAllLevelTables().Count(), Is.EqualTo(4));
            Assert.That(structure.GetAllLevelTables().Select(l => l.Id).ToHashSet().Single(), Is.EqualTo(questionnaireId));
            Assert.That(structure.GetAllLevelTables().Select(l => l.TableName).ToHashSet().Count, Is.EqualTo(4));
        }
    }
}
