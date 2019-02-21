using System;
using System.Collections.Generic;
using System.Data.Common;
using Npgsql;

namespace WB.Services.Export.InterviewDataStorage.InterviewDataExport
{
    public interface IInterviewDataExportCommandBuilder
    {
        DbCommand CreateUpdateValueForTable(string tableName, RosterTableKey rosterTableKey, IEnumerable<UpdateValueInfo> updateValueInfos);
        DbCommand CreateInsertInterviewCommandForTable(string tableName, IEnumerable<Guid> interviewIds);
        DbCommand CreateDeleteInterviewCommandForTable(string tableName, IEnumerable<Guid> interviewIds);
        DbCommand CreateAddRosterInstanceForTable(string tableName, IEnumerable<RosterTableKey> rosterInfos);
        DbCommand CreateRemoveRosterInstanceForTable(string tableName, IEnumerable<RosterTableKey> rosterInfos);
        List<DbCommand> BuildCommandsInExecuteOrderFromState(InterviewDataState state);
    }

    public interface IInterviewDataExportBulkCommandBuilder
    {
        NpgsqlCommand CreateUpdateValueForTable(NpgsqlCommand command, string tableName, RosterTableKey rosterTableKey, IEnumerable<UpdateValueInfo> updateValueInfos);
        NpgsqlCommand CreateInsertInterviewCommandForTable(NpgsqlCommand command, string tableName, IEnumerable<Guid> interviewIds);
        NpgsqlCommand CreateDeleteInterviewCommandForTable(NpgsqlCommand command, string tableName, IEnumerable<Guid> interviewIds);
        NpgsqlCommand CreateAddRosterInstanceForTable(NpgsqlCommand command, string tableName, IEnumerable<RosterTableKey> rosterInfos);
        NpgsqlCommand CreateRemoveRosterInstanceForTable(NpgsqlCommand command, string tableName, IEnumerable<RosterTableKey> rosterInfos);
        List<DbCommand> BuildCommandsInExecuteOrderFromState(InterviewDataState state);
    }
}
