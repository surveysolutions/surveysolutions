using System;
using System.Linq;
using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using ApprovalTests.Reporters.TestFrameworks;
using NpgsqlTypes;
using NUnit.Framework;
using WB.Services.Export.InterviewDataStorage.InterviewDataExport;
using WB.Services.Infrastructure.EventSourcing;

namespace WB.Services.Export.Tests.InterviewDataExport
{
    [UseApprovalSubdirectory("InterviewDataExportCommandBuilderTests-approved")]
    [IgnoreLineEndings(true)]
    [UseReporter(typeof(DiffReporter), typeof(NUnitReporter))]
    [TestOf(typeof(InterviewDataExportBulkCommandBuilder))]
    public class InterviewDataExportCommandBuilderTests
    {
        [Test]
        public void when_get_several_arguments_to_insert_interview_command()
        {
            var commandBuilder = CreateBuilder();
            ExportBulkCommand exportBulkCommand = new ExportBulkCommand();

            commandBuilder.AppendInsertInterviewCommandForTable(exportBulkCommand, fakeTableName1, new[] {interviewId1, interviewId2});
            var command = exportBulkCommand.GetCommand();

            Assert.That(command.Parameters.Count, Is.EqualTo(2));
            Assert.That(command.Parameters[0].Value, Is.EqualTo(interviewId1));
            Assert.That(command.Parameters[0].NpgsqlDbType, Is.EqualTo(NpgsqlDbType.Uuid));
            Assert.That(command.Parameters[1].Value, Is.EqualTo(interviewId2));
            Assert.That(command.Parameters[1].NpgsqlDbType, Is.EqualTo(NpgsqlDbType.Uuid));

            Approvals.Verify(command.CommandText);
        }

        [Test]
        public void when_get_several_arguments_to_remove_interview_command()
        {
            var commandBuilder = CreateBuilder();
            ExportBulkCommand exportBulkCommand = new ExportBulkCommand();

            commandBuilder.AppendDeleteInterviewCommandForTable(exportBulkCommand, fakeTableName1, new[] {interviewId1, interviewId2});
            var command = exportBulkCommand.GetCommand();

            Assert.That(command.Parameters.Count, Is.EqualTo(1));
            Assert.That(command.Parameters[0].Value, Is.EqualTo(new[] { interviewId1, interviewId2 }));
            Assert.That(command.Parameters[0].NpgsqlDbType, Is.EqualTo(NpgsqlDbType.Array | NpgsqlDbType.Uuid));

            Approvals.Verify(command.CommandText);
        }

        [Test]
        public void when_get_several_arguments_to_add_roster_instance_command()
        {
            var commandBuilder = CreateBuilder();
            ExportBulkCommand exportBulkCommand = new ExportBulkCommand();

            commandBuilder.AppendAddRosterInstanceForTable(
                exportBulkCommand,
                fakeTableName1, 
                new[]
                {
                    new RosterTableKey() { InterviewId = interviewId1, RosterVector = rosterVector1 }, 
                    new RosterTableKey() { InterviewId = interviewId1, RosterVector = rosterVector2 }, 
                    new RosterTableKey() { InterviewId = interviewId2, RosterVector = rosterVector1 }, 
                });
            var command = exportBulkCommand.GetCommand();

            Assert.That(command.Parameters.Count, Is.EqualTo(6));
            Assert.That(command.Parameters[0].Value, Is.EqualTo(interviewId1));
            Assert.That(command.Parameters[0].NpgsqlDbType, Is.EqualTo(NpgsqlDbType.Uuid));
            Assert.That(command.Parameters[1].Value, Is.EqualTo(rosterVector1));
            Assert.That(command.Parameters[1].NpgsqlDbType, Is.EqualTo(NpgsqlDbType.Array | NpgsqlDbType.Integer));
            Assert.That(command.Parameters[2].Value, Is.EqualTo(interviewId1));
            Assert.That(command.Parameters[2].NpgsqlDbType, Is.EqualTo(NpgsqlDbType.Uuid));
            Assert.That(command.Parameters[3].Value, Is.EqualTo(rosterVector2));
            Assert.That(command.Parameters[3].NpgsqlDbType, Is.EqualTo(NpgsqlDbType.Array | NpgsqlDbType.Integer));
            Assert.That(command.Parameters[4].Value, Is.EqualTo(interviewId2));
            Assert.That(command.Parameters[4].NpgsqlDbType, Is.EqualTo(NpgsqlDbType.Uuid));
            Assert.That(command.Parameters[5].Value, Is.EqualTo(rosterVector1));
            Assert.That(command.Parameters[5].NpgsqlDbType, Is.EqualTo(NpgsqlDbType.Array | NpgsqlDbType.Integer));

            Approvals.Verify(command.CommandText);
        }

        [Test]
        public void when_get_several_arguments_to_remove_roster_instance_command()
        {
            var commandBuilder = CreateBuilder();
            ExportBulkCommand exportBulkCommand = new ExportBulkCommand();

            commandBuilder.AppendRemoveRosterInstanceForTable(
                exportBulkCommand,
                fakeTableName1, 
                new[]
                {
                    new RosterTableKey() { InterviewId = interviewId1, RosterVector = rosterVector1 }, 
                    new RosterTableKey() { InterviewId = interviewId1, RosterVector = rosterVector2 }, 
                    new RosterTableKey() { InterviewId = interviewId2, RosterVector = rosterVector1 }, 
                });
            var command = exportBulkCommand.GetCommand();

            Assert.That(command.Parameters.Count, Is.EqualTo(6));
            Assert.That(command.Parameters[0].Value, Is.EqualTo(interviewId1));
            Assert.That(command.Parameters[0].NpgsqlDbType, Is.EqualTo(NpgsqlDbType.Uuid));
            Assert.That(command.Parameters[1].Value, Is.EqualTo(rosterVector1));
            Assert.That(command.Parameters[1].NpgsqlDbType, Is.EqualTo(NpgsqlDbType.Array | NpgsqlDbType.Integer));
            Assert.That(command.Parameters[2].Value, Is.EqualTo(interviewId1));
            Assert.That(command.Parameters[2].NpgsqlDbType, Is.EqualTo(NpgsqlDbType.Uuid));
            Assert.That(command.Parameters[3].Value, Is.EqualTo(rosterVector2));
            Assert.That(command.Parameters[3].NpgsqlDbType, Is.EqualTo(NpgsqlDbType.Array | NpgsqlDbType.Integer));
            Assert.That(command.Parameters[4].Value, Is.EqualTo(interviewId2));
            Assert.That(command.Parameters[4].NpgsqlDbType, Is.EqualTo(NpgsqlDbType.Uuid));
            Assert.That(command.Parameters[5].Value, Is.EqualTo(rosterVector1));
            Assert.That(command.Parameters[5].NpgsqlDbType, Is.EqualTo(NpgsqlDbType.Array | NpgsqlDbType.Integer));

            Approvals.Verify(command.CommandText);
        }

        [Test]
        public void when_get_several_arguments_to_update_row_in_same_table()
        {
            var commandBuilder = CreateBuilder();
            ExportBulkCommand exportBulkCommand = new ExportBulkCommand();

            commandBuilder.AppendUpdateValueForTable(
                exportBulkCommand,
                fakeTableName1, 
                new RosterTableKey() { InterviewId = interviewId1, RosterVector = rosterVector1 }, 
                new[]
                {
                    new UpdateValueInfo() { ColumnName = fakeColumnName1, Value = 11, ValueType = NpgsqlDbType.Integer }, 
                    new UpdateValueInfo() { ColumnName = fakeColumnName2, Value = "11", ValueType = NpgsqlDbType.Text }, 
                });
            var command = exportBulkCommand.GetCommand();

            Assert.That(command.Parameters.Count, Is.EqualTo(4));
            Assert.That(command.Parameters[0].Value, Is.EqualTo(11));
            Assert.That(command.Parameters[0].NpgsqlDbType, Is.EqualTo(NpgsqlDbType.Integer));
            Assert.That(command.Parameters[1].Value, Is.EqualTo("11"));
            Assert.That(command.Parameters[1].NpgsqlDbType, Is.EqualTo(NpgsqlDbType.Text));
            Assert.That(command.Parameters[2].Value, Is.EqualTo(interviewId1));
            Assert.That(command.Parameters[2].NpgsqlDbType, Is.EqualTo(NpgsqlDbType.Uuid));
            Assert.That(command.Parameters[3].Value, Is.EqualTo(rosterVector1));
            Assert.That(command.Parameters[3].NpgsqlDbType, Is.EqualTo(NpgsqlDbType.Array | NpgsqlDbType.Integer));

            Approvals.Verify(command.CommandText);
        }

        [Test]
        public void when_get_several_arguments_to_insert_interview_command_with_20_arguments()
        {
            var commandBuilder = CreateBuilder();
            ExportBulkCommand exportBulkCommand = new ExportBulkCommand();
            var interviewIds = Enumerable.Range(1, 20).Select(value => new Guid($"00000000-0000-0000-0000-00{value:0000000000}"));

            commandBuilder.AppendInsertInterviewCommandForTable(exportBulkCommand, fakeTableName1, interviewIds);
            var command = exportBulkCommand.GetCommand();

            Assert.That(command.Parameters.Count, Is.EqualTo(20));
            Approvals.Verify(command.CommandText);
        }

        [Test]
        public void when_get_several_arguments_to_add_roster_instance_command_with_20_arguments()
        {
            var commandBuilder = CreateBuilder();
            ExportBulkCommand exportBulkCommand = new ExportBulkCommand();

            var rosterInfos = Enumerable.Range(1, 20).Select(value =>
                new RosterTableKey() { InterviewId = new Guid($"00000000-0000-0000-0000-00{value:0000000000}"), RosterVector = rosterVector1 }
            );

            commandBuilder.AppendAddRosterInstanceForTable(
                exportBulkCommand,
                fakeTableName1, 
                rosterInfos);
            var command = exportBulkCommand.GetCommand();

            Assert.That(command.Parameters.Count, Is.EqualTo(40));
            Approvals.Verify(command.CommandText);
        }

        [Test]
        public void when_get_several_arguments_to_remove_roster_instance_command_with_20_arguments()
        {
            var commandBuilder = CreateBuilder();
            ExportBulkCommand exportBulkCommand = new ExportBulkCommand();
            var rosterInfos = Enumerable.Range(1, 20).Select(value =>
                new RosterTableKey() { InterviewId = new Guid($"00000000-0000-0000-0000-00{value:0000000000}"), RosterVector = rosterVector1 }
            );

            commandBuilder.AppendRemoveRosterInstanceForTable(
                exportBulkCommand,
                fakeTableName1,
                rosterInfos);
            var command = exportBulkCommand.GetCommand();

            Assert.That(command.Parameters.Count, Is.EqualTo(40));
            Approvals.Verify(command.CommandText);
        }

        [Test]
        public void when_get_several_arguments_to_update_row_in_same_table_with_20_arguments()
        {
            var commandBuilder = CreateBuilder();
            ExportBulkCommand exportBulkCommand = new ExportBulkCommand();

            var rosterTableKey = new RosterTableKey() { InterviewId = interviewId1, RosterVector = rosterVector1 };
            var updateValueInfos = Enumerable.Range(1, 20).Select(value =>
                new UpdateValueInfo() { ColumnName = fakeColumnName1, Value = value, ValueType = NpgsqlDbType.Integer }
            );

            commandBuilder.AppendUpdateValueForTable(
                exportBulkCommand,
                fakeTableName1, 
                rosterTableKey, 
                updateValueInfos);
            var command = exportBulkCommand.GetCommand();

            Assert.That(command.Parameters.Count, Is.EqualTo(34));
            Approvals.Verify(command.CommandText);
        }


        private IInterviewDataExportBulkCommandBuilder CreateBuilder() => new InterviewDataExportBulkCommandBuilder(
            new InterviewDataExportBulkCommandBuilderSettings()
        {
                MaxParametersCountInOneCommand = 5,
                NewLineAfterCommand = true,
        });

        private readonly string fakeTableName1 = "fakeTableName1";
        private readonly string fakeColumnName1 = "fakeColumnName1";
        private readonly string fakeColumnName2 = "fakeColumnName2";
        private readonly Guid interviewId1 = Guid.NewGuid();
        private readonly Guid interviewId2 = Guid.NewGuid();
        private readonly RosterVector rosterVector1 = new RosterVector(1, 1);
        private readonly RosterVector rosterVector2 = new RosterVector(2, 2);

    }
}
