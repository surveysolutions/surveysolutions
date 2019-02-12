using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Npgsql;
using NpgsqlTypes;
using WB.Services.Export.Infrastructure;

namespace WB.Services.Export.InterviewDataStorage.InterviewDataExport
{
    public class InterviewDataExportCommandBuilder : IInterviewDataExportCommandBuilder
    {
        private readonly ITenantContext tenantContext;

        public InterviewDataExportCommandBuilder(ITenantContext tenantContext)
        {
            this.tenantContext = tenantContext;
        }

        public DbCommand CreateUpdateValueForTable(string tableName, RosterInfo rosterInfo, IEnumerable<UpdateValueInfo> updateValueInfos)
        {
            bool isTopLevel = rosterInfo.RosterVector == null || rosterInfo.RosterVector.Length == 0;

            NpgsqlCommand updateCommand = new NpgsqlCommand();

            var setValues = string.Empty;

            int index = 0;
            foreach (var updateValueInfo in updateValueInfos)
            {
                index++;
                setValues += $"   \"{updateValueInfo.ColumnName}\" = @answer{index},";

                if (updateValueInfo.Value == null)
                    updateCommand.Parameters.AddWithValue($"@answer{index}", DBNull.Value);
                else
                    updateCommand.Parameters.AddWithValue($"@answer{index}", updateValueInfo.ValueType, updateValueInfo.Value);
            }
            setValues = setValues.TrimEnd(',');

            var text = $"UPDATE \"{tableName}\" " +
                       $"   SET {setValues}" +
                       $" WHERE {InterviewDatabaseConstants.InterviewId} = @interviewId";

            updateCommand.Parameters.AddWithValue("@interviewId", NpgsqlDbType.Uuid, rosterInfo.InterviewId);

            if (!isTopLevel)
            {
                text += $"   AND {InterviewDatabaseConstants.RosterVector} = @rosterVector;";
                updateCommand.Parameters.AddWithValue("@rosterVector", NpgsqlDbType.Array | NpgsqlDbType.Integer, rosterInfo.RosterVector.Coordinates.ToArray());
            }

            updateCommand.CommandText = text;
            return updateCommand;
        }


        public DbCommand CreateInsertCommandForTable(string tableName, HashSet<Guid> interviewIds)
        {
            var text = $"INSERT INTO \"{tableName}\" ({InterviewDatabaseConstants.InterviewId})" +
                       $"           VALUES ";
            NpgsqlCommand insertCommand = new NpgsqlCommand();

            int index = 0;
            foreach (var interviewId in interviewIds)
            {
                index++;
                text += $" (@interviewId{index}),";
                insertCommand.Parameters.AddWithValue($"@interviewId{index}", NpgsqlDbType.Uuid, interviewId);
            }

            text = text.TrimEnd(',');
            text += ";";

            insertCommand.CommandText = text;
            return insertCommand;
        }

        public DbCommand CreateDeleteCommandForTable(string tableName, HashSet<Guid> interviewIds)
        {
            var text = $"DELETE FROM \"{tableName}\" " +
                       $"      WHERE {InterviewDatabaseConstants.InterviewId} = ANY(@interviewIds);";
            NpgsqlCommand deleteCommand = new NpgsqlCommand(text);
            deleteCommand.Parameters.AddWithValue("@interviewIds", NpgsqlDbType.Array | NpgsqlDbType.Uuid, interviewIds.ToArray());
            return deleteCommand;
        }

        public DbCommand CreateAddRosterInstanceForTable(string tableName, IEnumerable<RosterInfo> rosterInfos)
        {
            var text = $"INSERT INTO \"{tableName}\" ({InterviewDatabaseConstants.InterviewId}, {InterviewDatabaseConstants.RosterVector})" +
                       $"           VALUES";

            NpgsqlCommand insertCommand = new NpgsqlCommand();
            int index = 0;
            foreach (var rosterInfo in rosterInfos)
            {
                index++;
                text += $"       (@interviewId{index}, @rosterVector{index}),";
                insertCommand.Parameters.AddWithValue($"@interviewId{index}", NpgsqlDbType.Uuid, rosterInfo.InterviewId);
                insertCommand.Parameters.AddWithValue($"@rosterVector{index}", NpgsqlDbType.Array | NpgsqlDbType.Integer, rosterInfo.RosterVector.Coordinates.ToArray());
            }

            text = text.TrimEnd(',');
            text += ";";

            insertCommand.CommandText = text;
            return insertCommand;
        }

        public DbCommand CreateRemoveRosterInstanceForTable(string tableName, IEnumerable<RosterInfo> rosterInfos)
        {
            var text = $"DELETE FROM \"{tableName}\" " +
                       $"      WHERE ";
            NpgsqlCommand deleteCommand = new NpgsqlCommand();

            int index = 0;
            foreach (var rosterInfo in rosterInfos)
            {
                index++;
                text += $" (" +
                        $"   {InterviewDatabaseConstants.InterviewId} = @interviewId{index}" +
                        $"   AND {InterviewDatabaseConstants.RosterVector} = @rosterVector{index}" +
                        $" ) " +
                        $" OR";
                deleteCommand.Parameters.AddWithValue($"@interviewId{index}", NpgsqlDbType.Uuid, rosterInfo.InterviewId);
                deleteCommand.Parameters.AddWithValue($"@rosterVector{index}", NpgsqlDbType.Array | NpgsqlDbType.Integer, rosterInfo.RosterVector.Coordinates.ToArray());
            }

            text = text.TrimEnd('O', 'R');
            text += ";";

            deleteCommand.CommandText = text;

            return deleteCommand;
        }
    }
}
