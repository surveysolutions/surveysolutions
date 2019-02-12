using System;
using System.Collections.Generic;
using NpgsqlTypes;
using WB.Services.Infrastructure.EventSourcing;

namespace WB.Services.Export.InterviewDataStorage.InterviewDataExport
{
    public class InterviewDataState
    {
        public IDictionary<string, HashSet<Guid>> InsertInterviews { get; set; } = new Dictionary<string, HashSet<Guid>>();
        public IDictionary<string, HashSet<RosterInfo>> InsertRosters { get; set; } = new Dictionary<string, HashSet<RosterInfo>>();
        public IDictionary<string, IDictionary<RosterInfo, IDictionary<string, UpdateValueInfo>>> UpdateValues = new Dictionary<string, IDictionary<RosterInfo, IDictionary<string, UpdateValueInfo>>>();
        public IDictionary<string, HashSet<RosterInfo>> RemoveRosters { get; set; } = new Dictionary<string, HashSet<RosterInfo>>();
        public IDictionary<string, HashSet<Guid>> RemoveInterviews { get; set; } = new Dictionary<string, HashSet<Guid>>();

        public void InsertInterviewInTable(string tableName, Guid interviewId)
        {
            if (!InsertInterviews.ContainsKey(tableName))
                InsertInterviews.Add(tableName, new HashSet<Guid>());
            InsertInterviews[tableName].Add(interviewId);
        }

        public void RemoveInterviewFromTable(string tableName, Guid interviewId)
        {
            if (!RemoveInterviews.ContainsKey(tableName))
                RemoveInterviews.Add(tableName, new HashSet<Guid>());
            RemoveInterviews[tableName].Add(interviewId);
        }

        public void InsertRosterInTable(string tableName, Guid interviewId, RosterVector rosterVector)
        {
            if (!InsertRosters.ContainsKey(tableName))
                InsertRosters.Add(tableName, new HashSet<RosterInfo>());
            InsertRosters[tableName].Add(new RosterInfo() { InterviewId = interviewId, RosterVector = rosterVector});
        }

        public void RemoveRosterFromTable(string tableName, Guid interviewId, RosterVector rosterVector)
        {
            if (!RemoveRosters.ContainsKey(tableName))
                RemoveRosters.Add(tableName, new HashSet<RosterInfo>());
            RemoveRosters[tableName].Add(new RosterInfo() { InterviewId = interviewId, RosterVector = rosterVector });
        }

        public void UpdateValueInTable(string tableName, Guid interviewId, RosterVector rosterVector, string columnName, object value, NpgsqlDbType valueType)
        {
            if (!UpdateValues.ContainsKey(tableName))
                UpdateValues.Add(tableName, new Dictionary<RosterInfo, IDictionary<string, UpdateValueInfo>>());
            var updateValueForTable = UpdateValues[tableName];
            var rosterInfo = new RosterInfo() {InterviewId = interviewId, RosterVector = rosterVector};
            if (!updateValueForTable.ContainsKey(rosterInfo))
                updateValueForTable.Add(rosterInfo, new Dictionary<string, UpdateValueInfo>());
            var updateValueForRosterInstance = updateValueForTable[rosterInfo];
            updateValueForRosterInstance[columnName] = new UpdateValueInfo() { ColumnName = columnName, Value = value, ValueType = valueType};
        }
    }
}
