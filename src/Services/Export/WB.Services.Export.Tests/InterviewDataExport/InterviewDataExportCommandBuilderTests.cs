using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using Npgsql;
using NpgsqlTypes;
using NUnit.Framework;
using WB.Services.Export.InterviewDataStorage.InterviewDataExport;
using WB.Services.Infrastructure.EventSourcing;

namespace WB.Services.Export.Tests.InterviewDataExport
{
    [UseApprovalSubdirectory("InterviewDataExportCommandBuilderTests-approved")]
    [IgnoreLineEndings(true)]
    [UseReporter(typeof(DiffReporter), typeof(NUnitReporter))]
    [TestOf(typeof(InterviewDataExportCommandBuilder))]
    public class InterviewDataExportCommandBuilderTests
    {
        [Test]
        public void when_get_several_arguments_to_insert_interview_command()
        {
            var commandBuilder = CreateBuilder();

            var command = commandBuilder.CreateInsertInterviewCommandForTable(fakeTableName1, new[] {interviewId1, interviewId2});

            Assert.That(command.Parameters.Count, Is.EqualTo(2));
            Assert.That(command.Parameters[0].Value, Is.EqualTo(interviewId1));
            Assert.That(((NpgsqlParameter)command.Parameters[0]).NpgsqlDbType, Is.EqualTo(NpgsqlDbType.Uuid));
            Assert.That(command.Parameters[1].Value, Is.EqualTo(interviewId2));
            Assert.That(((NpgsqlParameter)command.Parameters[1]).NpgsqlDbType, Is.EqualTo(NpgsqlDbType.Uuid));

            Approvals.Verify(command.CommandText);
        }

        [Test]
        public void when_get_several_arguments_to_remove_interview_command()
        {
            var commandBuilder = CreateBuilder();

            var command = commandBuilder.CreateDeleteInterviewCommandForTable(fakeTableName1, new[] {interviewId1, interviewId2});

            Assert.That(command.Parameters.Count, Is.EqualTo(1));
            Assert.That(command.Parameters[0].Value, Is.EqualTo(new[] { interviewId1, interviewId2 }));
            Assert.That(((NpgsqlParameter)command.Parameters[0]).NpgsqlDbType, Is.EqualTo(NpgsqlDbType.Array | NpgsqlDbType.Uuid));

            Approvals.Verify(command.CommandText);
        }

        [Test]
        public void when_get_several_arguments_to_add_roster_instance_command()
        {
            var commandBuilder = CreateBuilder();

            var command = commandBuilder.CreateAddRosterInstanceForTable(
                fakeTableName1, 
                new[]
                {
                    new RosterTableKey() { InterviewId = interviewId1, RosterVector = rosterVector1 }, 
                    new RosterTableKey() { InterviewId = interviewId1, RosterVector = rosterVector2 }, 
                    new RosterTableKey() { InterviewId = interviewId2, RosterVector = rosterVector1 }, 
                });

            Assert.That(command.Parameters.Count, Is.EqualTo(6));
            Assert.That(command.Parameters[0].Value, Is.EqualTo(interviewId1));
            Assert.That(((NpgsqlParameter)command.Parameters[0]).NpgsqlDbType, Is.EqualTo(NpgsqlDbType.Uuid));
            Assert.That(command.Parameters[1].Value, Is.EqualTo(rosterVector1));
            Assert.That(((NpgsqlParameter)command.Parameters[1]).NpgsqlDbType, Is.EqualTo(NpgsqlDbType.Array | NpgsqlDbType.Integer));
            Assert.That(command.Parameters[2].Value, Is.EqualTo(interviewId1));
            Assert.That(((NpgsqlParameter)command.Parameters[2]).NpgsqlDbType, Is.EqualTo(NpgsqlDbType.Uuid));
            Assert.That(command.Parameters[3].Value, Is.EqualTo(rosterVector2));
            Assert.That(((NpgsqlParameter)command.Parameters[3]).NpgsqlDbType, Is.EqualTo(NpgsqlDbType.Array | NpgsqlDbType.Integer));
            Assert.That(command.Parameters[4].Value, Is.EqualTo(interviewId2));
            Assert.That(((NpgsqlParameter)command.Parameters[4]).NpgsqlDbType, Is.EqualTo(NpgsqlDbType.Uuid));
            Assert.That(command.Parameters[5].Value, Is.EqualTo(rosterVector1));
            Assert.That(((NpgsqlParameter)command.Parameters[5]).NpgsqlDbType, Is.EqualTo(NpgsqlDbType.Array | NpgsqlDbType.Integer));

            Approvals.Verify(command.CommandText);
        }

        [Test]
        public void when_get_several_arguments_to_remove_roster_instance_command()
        {
            var commandBuilder = CreateBuilder();

            var command = commandBuilder.CreateRemoveRosterInstanceForTable(
                fakeTableName1, 
                new[]
                {
                    new RosterTableKey() { InterviewId = interviewId1, RosterVector = rosterVector1 }, 
                    new RosterTableKey() { InterviewId = interviewId1, RosterVector = rosterVector2 }, 
                    new RosterTableKey() { InterviewId = interviewId2, RosterVector = rosterVector1 }, 
                });

            Assert.That(command.Parameters.Count, Is.EqualTo(6));
            Assert.That(command.Parameters[0].Value, Is.EqualTo(interviewId1));
            Assert.That(((NpgsqlParameter)command.Parameters[0]).NpgsqlDbType, Is.EqualTo(NpgsqlDbType.Uuid));
            Assert.That(command.Parameters[1].Value, Is.EqualTo(rosterVector1));
            Assert.That(((NpgsqlParameter)command.Parameters[1]).NpgsqlDbType, Is.EqualTo(NpgsqlDbType.Array | NpgsqlDbType.Integer));
            Assert.That(command.Parameters[2].Value, Is.EqualTo(interviewId1));
            Assert.That(((NpgsqlParameter)command.Parameters[2]).NpgsqlDbType, Is.EqualTo(NpgsqlDbType.Uuid));
            Assert.That(command.Parameters[3].Value, Is.EqualTo(rosterVector2));
            Assert.That(((NpgsqlParameter)command.Parameters[3]).NpgsqlDbType, Is.EqualTo(NpgsqlDbType.Array | NpgsqlDbType.Integer));
            Assert.That(command.Parameters[4].Value, Is.EqualTo(interviewId2));
            Assert.That(((NpgsqlParameter)command.Parameters[4]).NpgsqlDbType, Is.EqualTo(NpgsqlDbType.Uuid));
            Assert.That(command.Parameters[5].Value, Is.EqualTo(rosterVector1));
            Assert.That(((NpgsqlParameter)command.Parameters[5]).NpgsqlDbType, Is.EqualTo(NpgsqlDbType.Array | NpgsqlDbType.Integer));

            Approvals.Verify(command.CommandText);
        }

        [Test]
        public void when_get_several_arguments_to_update_row_in_same_table()
        {
            var commandBuilder = CreateBuilder();

            var command = commandBuilder.CreateUpdateValueForTable(
                fakeTableName1, 
                new RosterTableKey() { InterviewId = interviewId1, RosterVector = rosterVector1 }, 
                new[]
                {
                    new UpdateValueInfo() { ColumnName = fakeColumnName1, Value = 11, ValueType = NpgsqlDbType.Integer }, 
                    new UpdateValueInfo() { ColumnName = fakeColumnName2, Value = "11", ValueType = NpgsqlDbType.Text }, 
                });

            Assert.That(command.Parameters.Count, Is.EqualTo(4));
            Assert.That(command.Parameters[0].Value, Is.EqualTo(11));
            Assert.That(((NpgsqlParameter)command.Parameters[0]).NpgsqlDbType, Is.EqualTo(NpgsqlDbType.Integer));
            Assert.That(command.Parameters[1].Value, Is.EqualTo("11"));
            Assert.That(((NpgsqlParameter)command.Parameters[1]).NpgsqlDbType, Is.EqualTo(NpgsqlDbType.Text));
            Assert.That(command.Parameters[2].Value, Is.EqualTo(interviewId1));
            Assert.That(((NpgsqlParameter)command.Parameters[2]).NpgsqlDbType, Is.EqualTo(NpgsqlDbType.Uuid));
            Assert.That(command.Parameters[3].Value, Is.EqualTo(rosterVector1));
            Assert.That(((NpgsqlParameter)command.Parameters[3]).NpgsqlDbType, Is.EqualTo(NpgsqlDbType.Array | NpgsqlDbType.Integer));

            Approvals.Verify(command.CommandText);
        }


        private IInterviewDataExportCommandBuilder CreateBuilder() => new InterviewDataExportCommandBuilder();

        private readonly string fakeTableName1 = "fakeTableName1";
        private readonly string fakeTableName2 = "fakeTableName2";
        private readonly string fakeColumnName1 = "fakeColumnName1";
        private readonly string fakeColumnName2 = "fakeColumnName2";
        private readonly Guid interviewId1 = Guid.NewGuid();
        private readonly Guid interviewId2 = Guid.NewGuid();
        private readonly RosterVector rosterVector1 = new RosterVector(1, 1);
        private readonly RosterVector rosterVector2 = new RosterVector(2, 2);

    }
}
