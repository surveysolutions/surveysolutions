using System;
using System.Collections.Generic;
using System.Data.Common;

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
}
