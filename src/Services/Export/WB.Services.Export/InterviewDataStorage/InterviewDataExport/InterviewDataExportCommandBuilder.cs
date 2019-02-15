using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using Npgsql;
using NpgsqlTypes;
using WB.Services.Export.Infrastructure;

namespace WB.Services.Export.InterviewDataStorage.InterviewDataExport
{
    public class InterviewDataExportCommandBuilder : IInterviewDataExportCommandBuilder
    {
        public DbCommand CreateUpdateValueForTable(string tableName, RosterTableKey rosterTableKey, IEnumerable<UpdateValueInfo> updateValueInfos)
        {
            bool isTopLevel = rosterTableKey.RosterVector == null || rosterTableKey.RosterVector.Length == 0;

            NpgsqlCommand updateCommand = new NpgsqlCommand();

            StringBuilder sql = new StringBuilder();
            sql.Append("UPDATE \"").Append(tableName).Append('"')
                .Append(" SET ");

            int index = 0;
            foreach (var updateValueInfo in updateValueInfos)
            {
                index++;
                sql.Append('"').Append(updateValueInfo.ColumnName).Append("\" = @answer").Append(index).Append(',');

                if (updateValueInfo.Value == null)
                    updateCommand.Parameters.AddWithValue($"@answer{index}", DBNull.Value);
                else
                    updateCommand.Parameters.AddWithValue($"@answer{index}", updateValueInfo.ValueType, updateValueInfo.Value);
            }

            sql.Remove(sql.Length - 1, 1); // TrimEnd(',');

            sql.Append(" WHERE ").Append(InterviewDatabaseConstants.InterviewId).Append(" = @interviewId");

            updateCommand.Parameters.AddWithValue("@interviewId", NpgsqlDbType.Uuid, rosterTableKey.InterviewId);

            if (!isTopLevel)
            {
                sql.Append(" AND ").Append(InterviewDatabaseConstants.RosterVector).Append(" = @rosterVector");
                updateCommand.Parameters.AddWithValue("@rosterVector", NpgsqlDbType.Array | NpgsqlDbType.Integer, rosterTableKey.RosterVector.Coordinates.ToArray());
            }

            sql.Append(';');

            updateCommand.CommandText = sql.ToString();
            return updateCommand;
        }


        public DbCommand CreateInsertInterviewCommandForTable(string tableName, IEnumerable<Guid> interviewIds)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append($"INSERT INTO \"").Append(tableName).Append("\" (").Append(InterviewDatabaseConstants.InterviewId).Append($")")
                .Append("           VALUES ");
            NpgsqlCommand insertCommand = new NpgsqlCommand();

            int index = 0;
            foreach (var interviewId in interviewIds)
            {
                index++;
                sql.Append(" (@interviewId").Append(index).Append("),");
                insertCommand.Parameters.AddWithValue($"@interviewId{index}", NpgsqlDbType.Uuid, interviewId);
            }

            sql.Remove(sql.Length - 1, 1); // TrimEnd(',');
            sql.Append(';');

            insertCommand.CommandText = sql.ToString();
            return insertCommand;
        }

        public DbCommand CreateDeleteInterviewCommandForTable(string tableName, IEnumerable<Guid> interviewIds)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("DELETE FROM \"").Append(tableName).Append('"')
                .Append(" WHERE ").Append(InterviewDatabaseConstants.InterviewId).Append(" = ANY(@interviewIds);");
            NpgsqlCommand deleteCommand = new NpgsqlCommand(sql.ToString());
            deleteCommand.Parameters.AddWithValue("@interviewIds", NpgsqlDbType.Array | NpgsqlDbType.Uuid, interviewIds.ToArray());
            return deleteCommand;
        }

        public DbCommand CreateAddRosterInstanceForTable(string tableName, IEnumerable<RosterTableKey> rosterInfos)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("INSERT INTO \"").Append(tableName).Append("\" (").Append(InterviewDatabaseConstants.InterviewId).Append(", ").Append(InterviewDatabaseConstants.RosterVector).Append(")")
                .Append(" VALUES");

            NpgsqlCommand insertCommand = new NpgsqlCommand();
            int index = 0;
            foreach (var rosterInfo in rosterInfos)
            {
                index++;
                sql.Append("(@interviewId").Append(index).Append(", @rosterVector").Append(index).Append("),");
                insertCommand.Parameters.AddWithValue($"@interviewId{index}", NpgsqlDbType.Uuid, rosterInfo.InterviewId);
                insertCommand.Parameters.AddWithValue($"@rosterVector{index}", NpgsqlDbType.Array | NpgsqlDbType.Integer, rosterInfo.RosterVector.Coordinates.ToArray());
            }

            sql.Remove(sql.Length - 1, 1); // TrimEnd(',');
            sql.Append(';');

            insertCommand.CommandText = sql.ToString();
            return insertCommand;
        }

        public DbCommand CreateRemoveRosterInstanceForTable(string tableName, IEnumerable<RosterTableKey> rosterInfos)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("DELETE FROM \"").Append(tableName).Append('"')
                .Append(" WHERE ");
            NpgsqlCommand deleteCommand = new NpgsqlCommand();

            int index = 0;
            foreach (var rosterInfo in rosterInfos)
            {
                index++;
                sql.Append(" (")
                    .Append(InterviewDatabaseConstants.InterviewId).Append(" = @interviewId").Append(index)
                    .Append(" AND ")
                    .Append(InterviewDatabaseConstants.RosterVector).Append(" = @rosterVector").Append(index)
                    .Append(')')
                    .Append(" OR");
                deleteCommand.Parameters.AddWithValue($"@interviewId{index}", NpgsqlDbType.Uuid, rosterInfo.InterviewId);
                deleteCommand.Parameters.AddWithValue($"@rosterVector{index}", NpgsqlDbType.Array | NpgsqlDbType.Integer, rosterInfo.RosterVector.Coordinates.ToArray());
            }

            sql.Remove(sql.Length - 2, 2); // TrimEnd('OR');
            sql.Append(';');

            deleteCommand.CommandText = sql.ToString();

            return deleteCommand;
        }

        public IEnumerable<DbCommand> BuildCommandsInExecuteOrderFromState(InterviewDataState state)
        {
            var commands = new List<DbCommand>();

            foreach (var tableWithAddInterviews in state.GetInsertInterviewsData())
                commands.Add(CreateInsertInterviewCommandForTable(tableWithAddInterviews.TableName, tableWithAddInterviews.InterviewIds));

            foreach (var tableWithAddRosters in state.GetRemoveRostersBeforeInsertNewInstancesData())
                commands.Add(CreateRemoveRosterInstanceForTable(tableWithAddRosters.TableName, tableWithAddRosters.RosterLevelInfo));

            foreach (var tableWithAddRosters in state.GetInsertRostersData())
                commands.Add(CreateAddRosterInstanceForTable(tableWithAddRosters.TableName, tableWithAddRosters.RosterLevelInfo));

            foreach (var updateValueInfo in state.GetUpdateValuesData())
            {
                var updateValueCommand = CreateUpdateValueForTable(updateValueInfo.TableName,
                    updateValueInfo.RosterLevelTableKey,
                    updateValueInfo.UpdateValuesInfo);
                commands.Add(updateValueCommand);
            }

            foreach (var tableWithRemoveRosters in state.GetRemoveRostersData())
                commands.Add(CreateRemoveRosterInstanceForTable(tableWithRemoveRosters.TableName, tableWithRemoveRosters.RosterLevelInfo));

            foreach (var tableWithRemoveInterviews in state.GetRemoveInterviewsData())
                commands.Add(CreateDeleteInterviewCommandForTable(tableWithRemoveInterviews.TableName, tableWithRemoveInterviews.InterviewIds));

            return commands;
        }
    }
}
