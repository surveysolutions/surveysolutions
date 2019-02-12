using System;
using System.Collections.Generic;
using System.Data.Common;

namespace WB.Services.Export.InterviewDataStorage.InterviewDataExport
{
    public interface IInterviewDataExportCommandBuilder
    {
        DbCommand CreateUpdateValueForTable(string tableName, RosterInfo rosterInfo, IEnumerable<UpdateValueInfo> updateValueInfos);
        DbCommand CreateInsertCommandForTable(string tableName, IEnumerable<Guid> interviewIds);
        DbCommand CreateDeleteCommandForTable(string tableName, IEnumerable<Guid> interviewIds);
        DbCommand CreateAddRosterInstanceForTable(string tableName, IEnumerable<RosterInfo> rosterInfos);
        DbCommand CreateRemoveRosterInstanceForTable(string tableName, IEnumerable<RosterInfo> rosterInfos);
    }
}
