using System;
using System.Linq;
using NpgsqlTypes;
using NUnit.Framework;
using WB.Services.Export.InterviewDataStorage.InterviewDataExport;
using WB.Services.Infrastructure.EventSourcing;

namespace WB.Services.Export.Tests.InterviewDataExport
{
    [TestOf(typeof(InterviewDataState))]
    public class InterviewDataStateTests
    {
        [Test]
        public void when_get_several_insert_interview_in_same_table_should_store_all_interviews()
        {
            var state = new InterviewDataState();

            state.InsertInterviewInTable(fakeTableName1, interviewId1);
            state.InsertInterviewInTable(fakeTableName1, interviewId2);
            state.InsertInterviewInTable(fakeTableName2, interviewId1);

            var insertData = state.GetInsertInterviewsData().ToList();

            Assert.That(insertData.Count, Is.EqualTo(2));
            var rowInfo1 = insertData.First();
            var rowInfo2 = insertData.Last();
            Assert.That(rowInfo1.TableName, Is.EqualTo(fakeTableName1));
            Assert.That(rowInfo2.TableName, Is.EqualTo(fakeTableName2));
            Assert.That(rowInfo1.InterviewIds.Count(), Is.EqualTo(2));
            Assert.That(rowInfo2.InterviewIds.Count(), Is.EqualTo(1));
            Assert.That(rowInfo1.InterviewIds, Is.EqualTo(new[] { interviewId1, interviewId2 }));
            Assert.That(rowInfo2.InterviewIds, Is.EqualTo(new[] { interviewId1 }));
        }

        [Test]
        public void when_get_several_answers_in_same_roster_instance_should_store_all_answers()
        {
            var state = new InterviewDataState();

            state.UpdateValueInTable(fakeTableName1, interviewId1, rosterVector1, fakeColumnName1, 1, NpgsqlDbType.Integer);
            state.UpdateValueInTable(fakeTableName1, interviewId1, rosterVector1, fakeColumnName2, "ttt", NpgsqlDbType.Text);

            var valuesData = state.GetUpdateValuesData().ToList();

            Assert.That(valuesData.Count, Is.EqualTo(1));
            var rowInfo = valuesData.Single();
            Assert.That(rowInfo.TableName, Is.EqualTo(fakeTableName1));
            Assert.That(rowInfo.RosterLevelTableKey.InterviewId, Is.EqualTo(interviewId1));
            Assert.That(rowInfo.RosterLevelTableKey.RosterVector, Is.EqualTo(rosterVector1));
            Assert.That(rowInfo.UpdateValuesInfo.Count(), Is.EqualTo(2));
            Assert.That(rowInfo.UpdateValuesInfo.First().ColumnName, Is.EqualTo(fakeColumnName1));
            Assert.That(rowInfo.UpdateValuesInfo.First().Value, Is.EqualTo(1));
            Assert.That(rowInfo.UpdateValuesInfo.First().ValueType, Is.EqualTo(NpgsqlDbType.Integer));
            Assert.That(rowInfo.UpdateValuesInfo.Last().ColumnName, Is.EqualTo(fakeColumnName2));
            Assert.That(rowInfo.UpdateValuesInfo.Last().Value, Is.EqualTo("ttt"));
            Assert.That(rowInfo.UpdateValuesInfo.Last().ValueType, Is.EqualTo(NpgsqlDbType.Text));
        }

        [Test]
        public void when_get_remove_interview_should_clear_all_insert_and_remove_operations()
        {
            var state = new InterviewDataState();

            state.InsertInterviewInTable(fakeTableName1, interviewId1);
            state.InsertRosterInTable(fakeTableName1, interviewId1, rosterVector1);
            state.InsertRosterInTable(fakeTableName1, interviewId1, rosterVector2);
            state.UpdateValueInTable(fakeTableName1, interviewId1, rosterVector1, fakeColumnName1, 1, NpgsqlDbType.Integer);
            state.RemoveRosterFromTable(fakeTableName1, interviewId1, rosterVector1);
            state.InsertRosterInTable(fakeTableName1, interviewId1, rosterVector1);
            state.UpdateValueInTable(fakeTableName1, interviewId1, rosterVector1, fakeColumnName2, 1, NpgsqlDbType.Integer);
            state.RemoveInterviewFromTable(fakeTableName1, interviewId1);

            var insertInterview = state.GetInsertInterviewsData().ToList();
            var removeBeforeInsert = state.GetRemoveRostersBeforeInsertNewInstancesData().ToList();
            var insert = state.GetInsertRostersData().ToList();
            var remove = state.GetRemoveRostersData().ToList();
            var updateValues = state.GetUpdateValuesData().ToList();
            var removeInterview = state.GetRemoveInterviewsData().ToList();

            Assert.That(insertInterview.Count, Is.EqualTo(0));
            Assert.That(removeBeforeInsert.Count, Is.EqualTo(0));
            Assert.That(insert.Count, Is.EqualTo(0));
            Assert.That(remove.Count, Is.EqualTo(0));
            Assert.That(updateValues.Count, Is.EqualTo(0));
            Assert.That(removeInterview.Count, Is.EqualTo(1));
            var rowInfo = removeInterview.Single();
            Assert.That(rowInfo.TableName, Is.EqualTo(fakeTableName1));
            Assert.That(rowInfo.InterviewIds.Count(), Is.EqualTo(1));
            Assert.That(rowInfo.InterviewIds.Single(), Is.EqualTo(interviewId1));
        }

        [Test]
        public void when_get_several_answers_on_same_question_for_same_table_should_store_only_last_answer_value()
        {
            var state = new InterviewDataState();

            state.UpdateValueInTable(fakeTableName1, interviewId1, rosterVector1, fakeColumnName1, "val1", NpgsqlDbType.Text);
            state.UpdateValueInTable(fakeTableName1, interviewId1, rosterVector1, fakeColumnName1, null, NpgsqlDbType.Text);
            state.UpdateValueInTable(fakeTableName1, interviewId1, rosterVector1, fakeColumnName1, "new val", NpgsqlDbType.Text);

            var valuesData = state.GetUpdateValuesData().ToList();

            Assert.That(valuesData.Count, Is.EqualTo(1));
            var rowInfo = valuesData.Single();
            Assert.That(rowInfo.TableName, Is.EqualTo(fakeTableName1));
            Assert.That(rowInfo.RosterLevelTableKey.InterviewId, Is.EqualTo(interviewId1));
            Assert.That(rowInfo.RosterLevelTableKey.RosterVector, Is.EqualTo(rosterVector1));
            Assert.That(rowInfo.UpdateValuesInfo.Count(), Is.EqualTo(1));
            Assert.That(rowInfo.UpdateValuesInfo.Single().Value, Is.EqualTo("new val"));
            Assert.That(rowInfo.UpdateValuesInfo.Single().ColumnName, Is.EqualTo(fakeColumnName1));
            Assert.That(rowInfo.UpdateValuesInfo.Single().ValueType, Is.EqualTo(NpgsqlDbType.Text));
        }

        [Test]
        public void when_get_several_answers_in_roster_and_then_get_remove_roster_should_dont_store_any_answer_value()
        {
            var state = new InterviewDataState();

            state.UpdateValueInTable(fakeTableName1, interviewId1, rosterVector1, fakeColumnName1, "val1", NpgsqlDbType.Text);
            state.UpdateValueInTable(fakeTableName1, interviewId1, rosterVector1, fakeColumnName2, null, NpgsqlDbType.Integer);
            state.RemoveRosterFromTable(fakeTableName1, interviewId1, rosterVector1);

            var valuesData = state.GetUpdateValuesData().ToList();

            Assert.That(valuesData.Count, Is.EqualTo(0));
        }

        [Test]
        public void when_get_several_answers_in_roster_and_then_get_remove_roster_should_store_only_remove_roster()
        {
            var state = new InterviewDataState();

            state.UpdateValueInTable(fakeTableName1, interviewId1, rosterVector1, fakeColumnName1, "val1", NpgsqlDbType.Text);
            state.UpdateValueInTable(fakeTableName1, interviewId1, rosterVector1, fakeColumnName2, null, NpgsqlDbType.Integer);
            state.RemoveRosterFromTable(fakeTableName1, interviewId1, rosterVector1);

            var valuesData = state.GetRemoveRostersData().ToList();

            Assert.That(valuesData.Count, Is.EqualTo(1));
            var tableInfo = valuesData.Single();
            Assert.That(tableInfo.TableName, Is.EqualTo(fakeTableName1));
            Assert.That(tableInfo.RosterLevelInfo.Count(), Is.EqualTo(1));
            Assert.That(tableInfo.RosterLevelInfo.Single().InterviewId, Is.EqualTo(interviewId1));
            Assert.That(tableInfo.RosterLevelInfo.Single().RosterVector, Is.EqualTo(rosterVector1));
        }

        [Test]
        public void when_get_remove_roster_and_get_insert_same_roster_again_should_store_remove_roster_before_insert_and_in_insert_collection()
        {
            var state = new InterviewDataState();
            var rosterTableKey = new RosterTableKey() {InterviewId = interviewId1, RosterVector = rosterVector1};
            state.UpdateValueInTable(fakeTableName1, interviewId1, rosterVector1, fakeColumnName1, "val1", NpgsqlDbType.Text);
            state.RemoveRosterFromTable(fakeTableName1, interviewId1, rosterVector1);
            state.InsertRosterInTable(fakeTableName1, interviewId1, rosterVector1);
            state.UpdateValueInTable(fakeTableName1, interviewId1, rosterVector1, fakeColumnName2, null, NpgsqlDbType.Integer);

            var removeBeforeInsert = state.GetRemoveRostersBeforeInsertNewInstancesData().ToList();
            var insert = state.GetInsertRostersData().ToList();
            var remove = state.GetRemoveRostersData().ToList();
            var updateValues = state.GetUpdateValuesData().ToList();

            Assert.That(removeBeforeInsert.Count, Is.EqualTo(1));
            Assert.That(removeBeforeInsert.Single().TableName, Is.EqualTo(fakeTableName1));
            Assert.That(removeBeforeInsert.Single().RosterLevelInfo.Single(), Is.EqualTo(rosterTableKey));

            Assert.That(insert.Count, Is.EqualTo(1));
            Assert.That(insert.Single().TableName, Is.EqualTo(fakeTableName1));
            Assert.That(insert.Single().RosterLevelInfo.Single(), Is.EqualTo(rosterTableKey));

            Assert.That(remove.Count, Is.EqualTo(0));

            Assert.That(updateValues.Count, Is.EqualTo(1));
            var tableInfo = updateValues.Single();
            Assert.That(tableInfo.TableName, Is.EqualTo(fakeTableName1));
            Assert.That(tableInfo.RosterLevelTableKey, Is.EqualTo(rosterTableKey));
            Assert.That(tableInfo.UpdateValuesInfo.Count(), Is.EqualTo(1));
            Assert.That(tableInfo.UpdateValuesInfo.Single().ColumnName, Is.EqualTo(fakeColumnName2));
            Assert.That(tableInfo.UpdateValuesInfo.Single().Value, Is.EqualTo(null));
            Assert.That(tableInfo.UpdateValuesInfo.Single().ValueType, Is.EqualTo(NpgsqlDbType.Integer));
        }

        private readonly string fakeTableName1  = "fakeTableName1";
        private readonly string fakeTableName2  = "fakeTableName2";
        private readonly string fakeColumnName1 = "fakeColumnName1";
        private readonly string fakeColumnName2 = "fakeColumnName2";
        private readonly Guid interviewId1      = Guid.NewGuid();
        private readonly Guid interviewId2      = Guid.NewGuid();
        private readonly RosterVector rosterVector1 = new RosterVector(1, 1);
        private readonly RosterVector rosterVector2 = new RosterVector(2, 2);
    }
}
