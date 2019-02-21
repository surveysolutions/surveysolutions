using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using Npgsql;
using NpgsqlTypes;

namespace WB.Services.Export.InterviewDataStorage.InterviewDataExport
{
    public class InterviewDataExportBulkCommandBuilder : IInterviewDataExportBulkCommandBuilder
    {
        public NpgsqlCommand CreateUpdateValueForTable(NpgsqlCommand command, string tableName, RosterTableKey rosterTableKey, IEnumerable<UpdateValueInfo> updateValueInfos)
        {
            bool isTopLevel = rosterTableKey.RosterVector == null || rosterTableKey.RosterVector.Length == 0;

            StringBuilder sql = new StringBuilder();
            sql.Append("UPDATE \"").Append(tableName).Append('"')
                .Append(" SET ");

            int index = command.Parameters.Count;
            foreach (var updateValueInfo in updateValueInfos)
            {
                index++;
                sql.Append('"').Append(updateValueInfo.ColumnName).Append("\" = @answer").Append(index).Append(',');

                if (updateValueInfo.Value == null)
                    command.Parameters.AddWithValue("@answer" + index, DBNull.Value);
                else
                    command.Parameters.AddWithValue("@answer" + index, updateValueInfo.ValueType, updateValueInfo.Value);
            }

            sql.Remove(sql.Length - 1, 1); // TrimEnd(',');

            sql.Append(" WHERE ").Append(InterviewDatabaseConstants.InterviewId).Append(" = @interviewId"+index);

            command.Parameters.AddWithValue("@interviewId"+index, NpgsqlDbType.Uuid, rosterTableKey.InterviewId);

            if (!isTopLevel)
            {
                sql.Append(" AND ").Append(InterviewDatabaseConstants.RosterVector).Append(" = @rosterVector"+index);
                command.Parameters.AddWithValue("@rosterVector"+index, 
                    NpgsqlDbType.Array | NpgsqlDbType.Integer, rosterTableKey.RosterVector.Coordinates.ToArray());
            }

            sql.Append(';');

            command.CommandText += sql.ToString();
            return command;
        }

        public NpgsqlCommand CreateInsertInterviewCommandForTable(NpgsqlCommand command, string tableName, IEnumerable<Guid> interviewIds)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("INSERT INTO \"").Append(tableName).Append("\" (").Append(InterviewDatabaseConstants.InterviewId).Append(")")
                .Append(" VALUES ");

            int index = command.Parameters.Count;
            foreach (var interviewId in interviewIds)
            {
                index++;
                sql.Append(" (@interviewId").Append(index).Append("),");
                command.Parameters.AddWithValue("@interviewId" + index, NpgsqlDbType.Uuid, interviewId);
            }

            sql.Remove(sql.Length - 1, 1); // TrimEnd(',');
            sql.Append(" ON CONFLICT DO NOTHING;");

            command.CommandText += sql.ToString();
            return command;
        }

        public NpgsqlCommand CreateDeleteInterviewCommandForTable(NpgsqlCommand command, string tableName, IEnumerable<Guid> interviewIds)
        {
            StringBuilder sql = new StringBuilder();
            int index = command.Parameters.Count;
            sql.Append("DELETE FROM \"").Append(tableName).Append('"')
                .Append(" WHERE ").Append(InterviewDatabaseConstants.InterviewId).Append(" = ANY(@interviewIds").Append(index).Append(");");
            command.CommandText += sql.ToString();
            command.Parameters.AddWithValue("@interviewIds"+index, NpgsqlDbType.Array | NpgsqlDbType.Uuid, interviewIds.ToArray());
            return command;
        }

        public NpgsqlCommand CreateAddRosterInstanceForTable(NpgsqlCommand command, string tableName, IEnumerable<RosterTableKey> rosterInfos)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("INSERT INTO \"").Append(tableName).Append("\" (").Append(InterviewDatabaseConstants.InterviewId).Append(", ").Append(InterviewDatabaseConstants.RosterVector).Append(")")
                .Append(" VALUES");

            int index = command.Parameters.Count;
            foreach (var rosterInfo in rosterInfos)
            {
                index++;
                sql.Append("(@interviewId").Append(index).Append(", @rosterVector").Append(index).Append("),");
                command.Parameters.AddWithValue("@interviewId" + index, NpgsqlDbType.Uuid, rosterInfo.InterviewId);
                command.Parameters.AddWithValue("@rosterVector" + index, NpgsqlDbType.Array | NpgsqlDbType.Integer, rosterInfo.RosterVector.Coordinates.ToArray());
            }

            sql.Remove(sql.Length - 1, 1); // TrimEnd(',');
            sql.Append(" ON CONFLICT DO NOTHING;");

            command.CommandText += sql.ToString();
            return command;
        }

        public NpgsqlCommand CreateRemoveRosterInstanceForTable(NpgsqlCommand command, string tableName, IEnumerable<RosterTableKey> rosterInfos)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("DELETE FROM \"").Append(tableName).Append('"')
                .Append(" WHERE ");

            int index = command.Parameters.Count;
            foreach (var rosterInfo in rosterInfos)
            {
                index++;
                sql.Append(" (")
                    .Append(InterviewDatabaseConstants.InterviewId).Append(" = @interviewId").Append(index)
                    .Append(" AND ")
                    .Append(InterviewDatabaseConstants.RosterVector).Append(" = @rosterVector").Append(index)
                    .Append(')')
                    .Append(" OR");
                command.Parameters.AddWithValue("@interviewId" + index, NpgsqlDbType.Uuid, rosterInfo.InterviewId);
                command.Parameters.AddWithValue("@rosterVector" + index, NpgsqlDbType.Array | NpgsqlDbType.Integer, rosterInfo.RosterVector.Coordinates.ToArray());
            }

            sql.Remove(sql.Length - 2, 2); // TrimEnd('OR');
            sql.Append(';');

            command.CommandText += sql.ToString();

            return command;
        }

        public List<DbCommand> BuildCommandsInExecuteOrderFromState(InterviewDataState state)
        {
            NpgsqlCommand command = new NpgsqlCommand();

            var commands = new List<DbCommand>();
            int lastCommandsCount = commands.Count;

            void Track(string commandType)
            {
                if(state.TenantName != null)
                    Monitoring.TrackSqlCommandsGeneration(state.TenantName, commandType, commands.Count - lastCommandsCount);
                lastCommandsCount = commands.Count;
            }

            foreach (var tableWithAddInterviews in state.GetInsertInterviewsData())
                command = CreateInsertInterviewCommandForTable(command, tableWithAddInterviews.TableName, tableWithAddInterviews.InterviewIds);

            Track("insert_interviews");

            foreach (var tableWithAddRosters in state.GetRemoveRostersBeforeInsertNewInstancesData())
                command = CreateRemoveRosterInstanceForTable(command, tableWithAddRosters.TableName, tableWithAddRosters.RosterLevelInfo);

            Track("delete_rosters_before_insert");

            foreach (var tableWithAddRosters in state.GetInsertRostersData())
                command = CreateAddRosterInstanceForTable(command, tableWithAddRosters.TableName, tableWithAddRosters.RosterLevelInfo);

            Track("insert_rosters");

            foreach (var update in state.GetUpdateValuesData())
                command = CreateUpdateValueForTable(command, update.TableName, update.RosterLevelTableKey, update.UpdateValuesInfo);
            
            Track("updates");

            foreach (var tableWithRemoveRosters in state.GetRemoveRostersData())
                command = CreateRemoveRosterInstanceForTable(command, tableWithRemoveRosters.TableName, tableWithRemoveRosters.RosterLevelInfo);

            Track("delete_rosters");

            foreach (var tableWithRemoveInterviews in state.GetRemoveInterviewsData())
                command = CreateDeleteInterviewCommandForTable(command, tableWithRemoveInterviews.TableName, tableWithRemoveInterviews.InterviewIds);

            Track("delete_interviews");

            return new List<DbCommand>() {command};
        }
    }
}
