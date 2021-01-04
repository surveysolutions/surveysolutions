using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using NpgsqlTypes;

namespace WB.Services.Export.InterviewDataStorage.InterviewDataExport
{
    public class InterviewDataExportBulkCommandBuilder : IInterviewDataExportBulkCommandBuilder
    {
        private readonly InterviewDataExportBulkCommandBuilderSettings settings;

        public InterviewDataExportBulkCommandBuilder(InterviewDataExportBulkCommandBuilderSettings settings)
        {
            this.settings = settings;
        }

        private int MaxParametersCountInOneCommand => settings.MaxParametersCountInOneCommand;

        public void AppendUpdateValueForTable(ExportBulkCommand command, string tableName, RosterTableKey rosterTableKey, IEnumerable<UpdateValueInfo> updateValueInfos)
        {
            void StartCommand() =>
                command.CommandText.Append("UPDATE \"").Append(tableName).Append('"')
                    .Append(" SET ");
            StartCommand();

            int parametersCount = 2; // reserve for 2 parameters on endCommand
            int index = command.Parameters.Count;
            foreach (var updateValueInfo in updateValueInfos)
            {
                if (parametersCount >= MaxParametersCountInOneCommand)
                {
                    EndCommand();
                    StartCommand();
                    parametersCount = 2;
                }

                index++;
                command.CommandText.Append('"').Append(updateValueInfo.ColumnName).Append("\" = @answer").Append(index).Append(',');

                if (updateValueInfo.Value == null)
                    command.Parameters.AddWithValue("@answer" + index, DBNull.Value);
                else
                    command.Parameters.AddWithValue("@answer" + index, updateValueInfo.ValueType, updateValueInfo.Value);

                parametersCount++;
            }

            void EndCommand()
            {
                command.CommandText.Remove(command.CommandText.Length - 1, 1); // TrimEnd(',');

                command.CommandText.Append(" WHERE ").Append(InterviewDatabaseConstants.InterviewId)
                    .Append(" = @interviewId" + index);

                command.Parameters.AddWithValue("@interviewId" + index, NpgsqlDbType.Uuid, rosterTableKey.InterviewId);

                bool isTopLevel = rosterTableKey.RosterVector == null || rosterTableKey.RosterVector.Length == 0;
                if (!isTopLevel)
                {
                    command.CommandText.Append(" AND ").Append(InterviewDatabaseConstants.RosterVector)
                        .Append(" = @rosterVector" + index);

                    command.Parameters.AddWithValue("@rosterVector" + index,
                        NpgsqlDbType.Array | NpgsqlDbType.Integer, 
                        rosterTableKey?.RosterVector?.Coordinates.ToArray() ?? Array.Empty<int>());
                }

                command.CommandText.Append(';');

                if (settings.NewLineAfterCommand)
                    command.CommandText.AppendLine();
            }
            EndCommand();
        }

        public void AppendInsertInterviewCommandForTable(ExportBulkCommand command, string tableName, IEnumerable<Guid> interviewIds)
        {
            void StartInsertCommand() =>
                command.CommandText.Append("INSERT INTO \"").Append(tableName).Append("\" (").Append(InterviewDatabaseConstants.InterviewId).Append(")")
                    .Append(" VALUES ");
            StartInsertCommand();

            int parametersCount = 0;
            int index = command.Parameters.Count;
            foreach (var interviewId in interviewIds)
            {
                if (parametersCount >= MaxParametersCountInOneCommand)
                {
                    EndCommand();
                    StartInsertCommand();
                    parametersCount = 0;
                }

                index++;
                command.CommandText.Append(" (@interviewId").Append(index).Append("),");
                command.Parameters.AddWithValue("@interviewId" + index, NpgsqlDbType.Uuid, interviewId);

                parametersCount++;
            }

            void EndCommand()
            {
                command.CommandText.Remove(command.CommandText.Length - 1, 1); // TrimEnd(',');
                command.CommandText.Append(" ON CONFLICT DO NOTHING;");

                if (settings.NewLineAfterCommand)
                    command.CommandText.AppendLine();
            }
            EndCommand();
        }

        public void AppendDeleteInterviewCommandForTable(ExportBulkCommand command, string tableName, IEnumerable<Guid> interviewIds)
        {
            int index = command.Parameters.Count;
            command.CommandText.Append("DELETE FROM \"").Append(tableName).Append('"')
                .Append(" WHERE ").Append(InterviewDatabaseConstants.InterviewId).Append(" = ANY(@interviewIds").Append(index).Append(");");
            command.Parameters.AddWithValue("@interviewIds"+index, NpgsqlDbType.Array | NpgsqlDbType.Uuid, interviewIds.ToArray());

            if (settings.NewLineAfterCommand)
                command.CommandText.AppendLine();
        }

        public void AppendAddRosterInstanceForTable(ExportBulkCommand command, string tableName, IEnumerable<RosterTableKey> rosterInfos)
        {
            void StartInsertCommand() =>
                command.CommandText.Append("INSERT INTO \"").Append(tableName).Append("\" (").Append(InterviewDatabaseConstants.InterviewId).Append(", ").Append(InterviewDatabaseConstants.RosterVector).Append(")")
                    .Append(" VALUES");
            StartInsertCommand();

            int parametersCount = 0;
            int index = command.Parameters.Count;
            foreach (var rosterInfo in rosterInfos)
            {
                if (parametersCount >= MaxParametersCountInOneCommand)
                {
                    EndCommand();
                    StartInsertCommand();
                    parametersCount = 0;
                }

                index++;
                command.CommandText.Append("(@interviewId").Append(index).Append(", @rosterVector").Append(index).Append("),");
                command.Parameters.AddWithValue("@interviewId" + index, NpgsqlDbType.Uuid, rosterInfo.InterviewId);
                command.Parameters.AddWithValue("@rosterVector" + index, NpgsqlDbType.Array | NpgsqlDbType.Integer, rosterInfo.RosterVector.Coordinates.ToArray());
                parametersCount += 2;
            }

            void EndCommand()
            {
                command.CommandText.Remove(command.CommandText.Length - 1, 1); // TrimEnd(',');
                command.CommandText.Append(" ON CONFLICT DO NOTHING;");

                if (settings.NewLineAfterCommand)
                    command.CommandText.AppendLine();
            }
            EndCommand();
        }

        public void AppendRemoveRosterInstanceForTable(ExportBulkCommand command, string tableName, IEnumerable<RosterTableKey> rosterInfos)
        {
            void StartCommand() =>
                command.CommandText.Append("DELETE FROM \"").Append(tableName).Append('"')
                    .Append(" WHERE ");
            StartCommand();

            int parametersCount = 0;
            int index = command.Parameters.Count;
            foreach (var rosterInfo in rosterInfos)
            {
                if (parametersCount >= MaxParametersCountInOneCommand)
                {
                    EndCommand();
                    StartCommand();
                    parametersCount = 0;
                }

                index++;
                command.CommandText.Append(" (")
                    .Append(InterviewDatabaseConstants.InterviewId).Append(" = @interviewId").Append(index)
                    .Append(" AND ")
                    .Append(InterviewDatabaseConstants.RosterVector).Append(" = @rosterVector").Append(index)
                    .Append(')')
                    .Append(" OR");
                command.Parameters.AddWithValue("@interviewId" + index, NpgsqlDbType.Uuid, rosterInfo.InterviewId);
                command.Parameters.AddWithValue("@rosterVector" + index, NpgsqlDbType.Array | NpgsqlDbType.Integer, rosterInfo.RosterVector.Coordinates.ToArray());
                parametersCount += 2;
            }

            void EndCommand()
            {
                command.CommandText.Remove(command.CommandText.Length - 2, 2); // TrimEnd('OR');
                command.CommandText.Append(';');

                if (settings.NewLineAfterCommand)
                    command.CommandText.AppendLine();
            }
            EndCommand();
        }

        public DbCommand BuildCommandsInExecuteOrderFromState(InterviewDataState state)
        {
            ExportBulkCommand command = new ExportBulkCommand();

            foreach (var tableWithAddInterviews in state.GetInsertInterviewsData())
                AppendInsertInterviewCommandForTable(command, tableWithAddInterviews.TableName, tableWithAddInterviews.InterviewIds);

            foreach (var tableWithAddRosters in state.GetRemoveRostersBeforeInsertNewInstancesData())
                AppendRemoveRosterInstanceForTable(command, tableWithAddRosters.TableName, tableWithAddRosters.RosterLevelInfo);

            foreach (var tableWithAddRosters in state.GetInsertRostersData())
                AppendAddRosterInstanceForTable(command, tableWithAddRosters.TableName, tableWithAddRosters.RosterLevelInfo);

            foreach (var update in state.GetUpdateValuesData())
                AppendUpdateValueForTable(command, update.TableName, update.RosterLevelTableKey, update.UpdateValuesInfo);
            
            foreach (var tableWithRemoveRosters in state.GetRemoveRostersData())
                AppendRemoveRosterInstanceForTable(command, tableWithRemoveRosters.TableName, tableWithRemoveRosters.RosterLevelInfo);

            foreach (var tableWithRemoveInterviews in state.GetRemoveInterviewsData())
                AppendDeleteInterviewCommandForTable(command, tableWithRemoveInterviews.TableName, tableWithRemoveInterviews.InterviewIds);

            return command.GetCommand();
        }
    }
}
