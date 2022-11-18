using System;
using System.Collections.Generic;
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

            Assert.That(structure.GetAllLevelTables().Count(), Is.EqualTo(5));
            Assert.That(structure.GetAllLevelTables().Select(l => l.Id).ToHashSet().Single(), Is.EqualTo(questionnaireId));
            Assert.That(structure.GetAllLevelTables().Select(l => l.TableName).ToHashSet().Count, Is.EqualTo(5));
        }


        [TestCase("12345678901234567890123456789012", // questionnaireVariable
                  "12345678901234567890123456789012", // rosterVariable
                  "EREREREREREREREREREREQ$2222_IiIiIiIiIiIiIiIiIiIiIg$400", // data table name
                  "EREREREREREREREREREREQ$2222_IiIiIiIiIiIiIiIiIiIiIg$400-e", // enablement table name
                  "EREREREREREREREREREREQ$2222_IiIiIiIiIiIiIiIiIiIiIg$400-v")] // validity table name
        [TestCase("12345678901234567890123456789012", // questionnaireVariable
                  "1234567890123456789012", // rosterVariable
                  "EREREREREREREREREREREQ$2222_1234567890123456789012$400", // data table name
                  "EREREREREREREREREREREQ$2222_1234567890123456789012$400-e", // enablement table name
                  "EREREREREREREREREREREQ$2222_1234567890123456789012$400-v")] // validity table name
        [TestCase("1234567890123456789012", // questionnaireVariable
                  "12345678901234567890123456789012", // rosterVariable
                  "1234567890123456789012$2222_IiIiIiIiIiIiIiIiIiIiIg$400", // data table name
                  "1234567890123456789012$2222_IiIiIiIiIiIiIiIiIiIiIg$400-e", // enablement table name
                  "1234567890123456789012$2222_IiIiIiIiIiIiIiIiIiIiIg$400-v")] // validity table name
        [TestCase("1234567890123456789012", // questionnaireVariable
                  "1234567890123456789012", // rosterVariable
                  "1234567890123456789012$2222_1234567890123456789012$400", // data table name
                  "1234567890123456789012$2222_1234567890123456789012$400-e", // enablement table name
                  "1234567890123456789012$2222_1234567890123456789012$400-v")] // validity table name
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

            Assert.That(structure.GetAllLevelTables().Count(), Is.EqualTo(402));
            var last = structure.GetAllLevelTables().Last();
            Assert.That(last.TableName, Is.EqualTo(dataTableName));
            Assert.That(last.EnablementTableName, Is.EqualTo(enablementTableName));
            Assert.That(last.ValidityTableName, Is.EqualTo(validityTableName));
        }

        [Test]
        public void when_questionnaire_has_entities_with_postgress_system_columns_should_generate_escaped_column_names()
        {
            var questionnaireVariable = "questionnaireVariable";
            var questionnaireId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var rosterVariable = "rosterVariable";
            var rosterId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var id3 = Guid.Parse("33333333333333333333333333333333");
            var id4 = Guid.Parse("44444444444444444444444444444444");
            var id5 = Guid.Parse("55555555555555555555555555555555");
            var id6 = Guid.Parse("66666666666666666666666666666666");
            var id7 = Guid.Parse("77777777777777777777777777777777");
            var id8 = Guid.Parse("88888888888888888888888888888888");
            var id9 = Guid.Parse("99999999999999999999999999999999");
            var id10 = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");

            var questionnaire = Create.QuestionnaireDocument(questionnaireId, version: 2222, variableName: questionnaireVariable, 
                children: Create.Roster(rosterId, variable: rosterVariable, 
                    children: new IQuestionnaireEntity[] {
                    Create.TextQuestion(id3, variable: "oid"),
                    Create.TextQuestion(id4, variable: "tableOid"),
                    Create.Variable(id5, name: "xMin"),
                    Create.Group(id6, variable: "cmin", children: new IQuestionnaireEntity[]
                    {
                        Create.Variable(id7, name: "XMAX"),
                        Create.TextQuestion(id8, variable:"cmax")
                    }),
                    Create.Variable(id9, name: "ctid"),
                    Create.TextQuestion(id10, variable:"normal_Name")
                }));

            Assert.That(questionnaire.Find<Question>(id3).ColumnName, Is.EqualTo("oid__1"));
            Assert.That(questionnaire.Find<Question>(id4).ColumnName, Is.EqualTo("tableoid__1"));
            Assert.That(questionnaire.Find<Variable>(id5).ColumnName, Is.EqualTo("xmin__1"));
            Assert.That(questionnaire.Find<Group>(id6).ColumnName, Is.EqualTo("cmin__1"));
            Assert.That(questionnaire.Find<Variable>(id7).ColumnName, Is.EqualTo("xmax__1"));
            Assert.That(questionnaire.Find<Question>(id8).ColumnName, Is.EqualTo("cmax__1"));
            Assert.That(questionnaire.Find<Variable>(id9).ColumnName, Is.EqualTo("ctid__1"));
            Assert.That(questionnaire.Find<Question>(id10).ColumnName, Is.EqualTo("normal_name"));
        }

        [Test]
        public void should_return_correct_table_name_for_large_sections()
        {
            List<IQuestionnaireEntity> childrenInGroup1 = new List<IQuestionnaireEntity>();
            List<IQuestionnaireEntity> childrenInGroup2 = new List<IQuestionnaireEntity>();
            for (int i = 0; i < 1000; i++)
            {
                childrenInGroup1.Add(Create.NumericIntegerQuestion(Guid.NewGuid(), variable: "num_group_1_" + i));
                childrenInGroup2.Add(Create.NumericIntegerQuestion(Guid.NewGuid(), variable: "num_group_2_" + i));
            }

            var questionnaire = Create.QuestionnaireDocument(id: Id.gA, version: 5, variableName: "questionnaire",
                children: new[]
                {
                    Create.Group(Id.g1, variable: "group_1", children: childrenInGroup1.ToArray()),
                    Create.Group(Id.g2, variable: "group_2", children: childrenInGroup2.ToArray())
                });

            // Act 
            var group = questionnaire.GetGroup(Id.g2);

            // Assert
            Assert.That(group.TableName, Is.EqualTo("questionnaire$5$1"));
            Assert.That(group.EnablementTableName, Is.EqualTo("questionnaire$5$1-e"));
            Assert.That(group.ValidityTableName, Is.EqualTo("questionnaire$5$1-v"));
        }
        
        [Ignore("bug with collision questionnaire variable, the same variable name now declined on HQ")]
        [Test]
        public void when_two_questionnaires_have_same_variable_should_generate_different_table_names()
        {
            var questionnaire1 = Create.QuestionnaireDocument(id: Id.gA, version: 1, variableName: "questionnaire",
                children: new[]
                {
                    Create.Roster(Id.g1, variable: "group"),
                });

            var questionnaire2 = Create.QuestionnaireDocument(id: Id.gB, version: 1, variableName: "questionnaire",
                children: new[]
                {
                    Create.Roster(Id.g1, variable: "group"),
                });

            // Act 
            var group1 = questionnaire1.GetGroup(Id.g1);
            var group2 = questionnaire2.GetGroup(Id.g1);

            // Assert
            Assert.That(group1.TableName, Is.EqualTo("questionnaire$1_group"));
            Assert.That(group1.EnablementTableName, Is.EqualTo("questionnaire$1_group-e"));
            Assert.That(group1.ValidityTableName, Is.EqualTo("questionnaire$1_group-v"));
            Assert.That(group2.TableName, Is.EqualTo("questionnaire$1_group"));
            Assert.That(group2.EnablementTableName, Is.EqualTo("questionnaire$1_group-e"));
            Assert.That(group2.ValidityTableName, Is.EqualTo("questionnaire$1_group-v"));
            Assert.That(group1.TableName, Is.Not.EqualTo(group2.TableName));
            Assert.That(group1.EnablementTableName, Is.Not.EqualTo(group2.EnablementTableName));
            Assert.That(group1.ValidityTableName, Is.Not.EqualTo(group2.ValidityTableName));
        }
        
        [Ignore("bug with collision table name")]
        [Test]
        public void when_generate_tables_should_have_unique_names()
        {
            var questionnaire = Create.QuestionnaireDocument(id: Id.gA, version: 1, variableName: "questionnaire",
                children: new[]
                {
                    Create.Roster(Id.g1, variable: "very long variable name for use guid as part of table name"),
                    Create.Roster(Id.g2, variable: "EREREREREREREREREREREQ")
                });

            // Act 
            var group1 = questionnaire.GetGroup(Id.g1);
            var group2 = questionnaire.GetGroup(Id.g2);

            // Assert
            Assert.That(group1.TableName, Is.EqualTo("questionnaire$1_EREREREREREREREREREREQ"));
            Assert.That(group1.EnablementTableName, Is.EqualTo("questionnaire$1_EREREREREREREREREREREQ-e"));
            Assert.That(group1.ValidityTableName, Is.EqualTo("questionnaire$1_EREREREREREREREREREREQ-v"));
            Assert.That(group2.TableName, Is.EqualTo("questionnaire$1_EREREREREREREREREREREQ"));
            Assert.That(group2.EnablementTableName, Is.EqualTo("questionnaire$1_EREREREREREREREREREREQ-e"));
            Assert.That(group2.ValidityTableName, Is.EqualTo("questionnaire$1_EREREREREREREREREREREQ-v"));
            Assert.That(group1.TableName, Is.Not.EqualTo(group2.TableName));
            Assert.That(group1.EnablementTableName, Is.Not.EqualTo(group2.EnablementTableName));
            Assert.That(group1.ValidityTableName, Is.Not.EqualTo(group2.ValidityTableName));
        }

    }
}
