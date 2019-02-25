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
                children: Enumerable.Range(0, 1300)
                    .Select(index => Create.Group(variable: "group" + index, 
                        children: Create.TextQuestion(variableLabel: "t" + index)))
                    .Cast<IQuestionnaireEntity>()
                    .ToArray());

            var structure = new QuestionnaireDatabaseStructure(questionnaire);

            Assert.That(structure.GetAllLevelTables().Count(), Is.EqualTo(2));
            Assert.That(structure.GetAllLevelTables().Select(l => l.Id).ToHashSet().Single(), Is.EqualTo(questionnaireId));
            Assert.That(structure.GetAllLevelTables().Select(l => l.TableName).ToHashSet().Count, Is.EqualTo(2));
        }


        [TestCase("12345678901234567890123456789012", // questionnaireVariable
                  "12345678901234567890123456789012", // rosterVariable
                  "EREREREREREREREREREREQ$2222_IiIiIiIiIiIiIiIiIiIiIg$133", // data table name
                  "EREREREREREREREREREREQ$2222_IiIiIiIiIiIiIiIiIiIiIg$133-e", // enablement table name
                  "EREREREREREREREREREREQ$2222_IiIiIiIiIiIiIiIiIiIiIg$133-v")] // validity table name
        [TestCase("12345678901234567890123456789012", // questionnaireVariable
                  "1234567890123456789012", // rosterVariable
                  "EREREREREREREREREREREQ$2222_1234567890123456789012$133", // data table name
                  "EREREREREREREREREREREQ$2222_1234567890123456789012$133-e", // enablement table name
                  "EREREREREREREREREREREQ$2222_1234567890123456789012$133-v")] // validity table name
        [TestCase("1234567890123456789012", // questionnaireVariable
                  "12345678901234567890123456789012", // rosterVariable
                  "1234567890123456789012$2222_IiIiIiIiIiIiIiIiIiIiIg$133", // data table name
                  "1234567890123456789012$2222_IiIiIiIiIiIiIiIiIiIiIg$133-e", // enablement table name
                  "1234567890123456789012$2222_IiIiIiIiIiIiIiIiIiIiIg$133-v")] // validity table name
        [TestCase("1234567890123456789012", // questionnaireVariable
                  "1234567890123456789012", // rosterVariable
                  "1234567890123456789012$2222_1234567890123456789012$133", // data table name
                  "1234567890123456789012$2222_1234567890123456789012$133-e", // enablement table name
                  "1234567890123456789012$2222_1234567890123456789012$133-v")] // validity table name
        public void when_questionnaire_has__bif_variable_label_and_roster_too_should_generate_table_name_max_64_chars(
            string questionnaireVariable, 
            string rosterVariable,
            string dataTableName,
            string enablementTableName,
            string validityTableName)
        {
            var questionnaireId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var rosterId = Guid.Parse("22222222-2222-2222-2222-222222222222");

            var questionnaire = Create.QuestionnaireDocument(questionnaireId, version: 2222, variableName: questionnaireVariable, 
                children: Create.Roster(rosterId, variable: rosterVariable, 
                        children: Enumerable.Range(0, 400)
                            .Select(index => Create.Group(variable: "group" + index,
                                children: Enumerable.Range(0, 400).Select(qindex => Create.TextQuestion(variableLabel: "t" + index + "-"+ qindex))
                                    .Cast<IQuestionnaireEntity>()
                                    .ToArray())
                            ).ToList()
                ));

            var structure = new QuestionnaireDatabaseStructure(questionnaire);

            Assert.That(structure.GetAllLevelTables().Count(), Is.EqualTo(135));
            var last = structure.GetAllLevelTables().Last();
            Assert.That(last.TableName, Is.EqualTo(dataTableName));
            Assert.That(last.EnablementTableName, Is.EqualTo(enablementTableName));
            Assert.That(last.ValidityTableName, Is.EqualTo(validityTableName));
        }
    }
}
