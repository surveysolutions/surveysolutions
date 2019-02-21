using System;
using System.Collections.Generic;
using System.Data.Common;
using Npgsql;

namespace WB.Services.Export.InterviewDataStorage.InterviewDataExport
{
    public interface IInterviewDataExportBulkCommandBuilder
    {
        void AppendUpdateValueForTable(ExportBulkCommand command, string tableName, RosterTableKey rosterTableKey, IEnumerable<UpdateValueInfo> updateValueInfos);
        void AppendInsertInterviewCommandForTable(ExportBulkCommand command, string tableName, IEnumerable<Guid> interviewIds);
        void AppendDeleteInterviewCommandForTable(ExportBulkCommand command, string tableName, IEnumerable<Guid> interviewIds);
        void AppendAddRosterInstanceForTable(ExportBulkCommand command, string tableName, IEnumerable<RosterTableKey> rosterInfos);
        void AppendRemoveRosterInstanceForTable(ExportBulkCommand command, string tableName, IEnumerable<RosterTableKey> rosterInfos);
        DbCommand BuildCommandsInExecuteOrderFromState(InterviewDataState state);
    }
}
