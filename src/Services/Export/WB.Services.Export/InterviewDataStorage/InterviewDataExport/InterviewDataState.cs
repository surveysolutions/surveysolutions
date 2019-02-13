using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using NpgsqlTypes;
using WB.Services.Infrastructure.EventSourcing;

namespace WB.Services.Export.InterviewDataStorage.InterviewDataExport
{
    public class InterviewDataState
    {
        private IDictionary<string, HashSet<Guid>> InsertInterviews { get; set; } = new Dictionary<string, HashSet<Guid>>();
        private IDictionary<string, HashSet<Guid>> RemoveInterviews { get; set; } = new Dictionary<string, HashSet<Guid>>();
        private IDictionary<string, HashSet<RosterInfo>> InsertRosters { get; set; } = new Dictionary<string, HashSet<RosterInfo>>();
        private IDictionary<string, HashSet<RosterInfo>> RemoveRostersBeforeInsertRosters { get; set; } = new Dictionary<string, HashSet<RosterInfo>>();
        private IDictionary<string, HashSet<RosterInfo>> RemoveRosters { get; set; } = new Dictionary<string, HashSet<RosterInfo>>();
        private IDictionary<string, IDictionary<RosterInfo, IDictionary<string, UpdateValueInfo>>> UpdateValues = new Dictionary<string, IDictionary<RosterInfo, IDictionary<string, UpdateValueInfo>>>();

        public void InsertInterviewInTable(string tableName, Guid interviewId)
        {
            if (!InsertInterviews.ContainsKey(tableName))
                InsertInterviews.Add(tableName, new HashSet<Guid>());
            InsertInterviews[tableName].Add(interviewId);
        }

        public IEnumerable<InterviewTableInfo> GetInsertInterviewsData()
        {
            return InsertInterviews
                .Where(kv => kv.Value.Any())
                .Select(kv => new InterviewTableInfo() { TableName = kv.Key, InterviewIds = kv.Value});
        }

        public void RemoveInterviewFromTable(string tableName, Guid interviewId)
        {
            if (InsertInterviews.ContainsKey(tableName)) InsertInterviews[tableName].Remove(interviewId);
            if (InsertRosters.ContainsKey(tableName)) InsertRosters[tableName].RemoveWhere(r => r.InterviewId == interviewId);
            if (RemoveRosters.ContainsKey(tableName)) RemoveRosters[tableName].RemoveWhere(r => r.InterviewId == interviewId);
            if (RemoveRostersBeforeInsertRosters.ContainsKey(tableName)) RemoveRostersBeforeInsertRosters[tableName].RemoveWhere(r => r.InterviewId == interviewId);
            if (UpdateValues.ContainsKey(tableName))
            {
                var rows = UpdateValues[tableName];
                var rosterInfos = rows.Keys.Where(r => r.InterviewId == interviewId).ToList();
                rosterInfos.ForEach(ri => rows.Remove(ri));
            }

            if (!RemoveInterviews.ContainsKey(tableName))
                RemoveInterviews.Add(tableName, new HashSet<Guid>());
            RemoveInterviews[tableName].Add(interviewId);
        }

        public IEnumerable<InterviewTableInfo> GetRemoveInterviewsData()
        {
            return RemoveInterviews
                .Where(kv => kv.Value.Any())
                .Select(kv => new InterviewTableInfo() { TableName = kv.Key, InterviewIds = kv.Value });
        }

        public void InsertRosterInTable(string tableName, Guid interviewId, RosterVector rosterVector)
        {
            var rosterInfo = new RosterInfo() { InterviewId = interviewId, RosterVector = rosterVector };
            var isExistsRemoveOperation = RemoveRosters.ContainsKey(tableName) && RemoveRosters[tableName].Contains(rosterInfo);
            if (isExistsRemoveOperation)
            {
                if (!RemoveRostersBeforeInsertRosters.ContainsKey(tableName))
                    RemoveRostersBeforeInsertRosters.Add(tableName, new HashSet<RosterInfo>());
                RemoveRostersBeforeInsertRosters[tableName].Add(rosterInfo);

                RemoveRosters[tableName].Remove(rosterInfo);
            }

            if (!InsertRosters.ContainsKey(tableName))
                InsertRosters.Add(tableName, new HashSet<RosterInfo>());
            InsertRosters[tableName].Add(rosterInfo);
        }

        public IEnumerable<RosterLevelTableInfo> GetRemoveRostersBeforeInsertNewInstancesData()
        {
            return RemoveRostersBeforeInsertRosters
                .Where(kv => kv.Value.Any())
                .Select(r => new RosterLevelTableInfo() { TableName = r.Key, RosterLevelInfo = r.Value });
        }

        public IEnumerable<RosterLevelTableInfo> GetInsertRostersData()
        {
            return InsertRosters
                .Where(kv => kv.Value.Any())
                .Select(r => new RosterLevelTableInfo() {TableName = r.Key, RosterLevelInfo = r.Value});
        }

        public void RemoveRosterFromTable(string tableName, Guid interviewId, RosterVector rosterVector)
        {
            var rosterInfo = new RosterInfo() {InterviewId = interviewId, RosterVector = rosterVector};

            if (InsertRosters.ContainsKey(tableName)) InsertRosters[tableName].Remove(rosterInfo);
            if (RemoveRostersBeforeInsertRosters.ContainsKey(tableName)) RemoveRostersBeforeInsertRosters[tableName].Remove(rosterInfo);
            if (UpdateValues.ContainsKey(tableName)) UpdateValues[tableName].Remove(rosterInfo);

            if (!RemoveRosters.ContainsKey(tableName))
                RemoveRosters.Add(tableName, new HashSet<RosterInfo>());
            RemoveRosters[tableName].Add(rosterInfo);
        }

        public IEnumerable<RosterLevelTableInfo> GetRemoveRostersData()
        {
            return RemoveRosters
                .Where(kv => kv.Value.Any())
                .Select(r => new RosterLevelTableInfo() { TableName = r.Key, RosterLevelInfo = r.Value });
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

        public IEnumerable<UpdateValueForTableRowInfo> GetUpdateValuesData()
        {
            foreach (var updateValueInfo in UpdateValues)
            {
                foreach (var groupedByInterviewAndRoster in updateValueInfo.Value)
                {
                    yield return new UpdateValueForTableRowInfo()
                    {
                        TableName = updateValueInfo.Key,
                        RosterLevelInfo = groupedByInterviewAndRoster.Key,
                        UpdateValuesInfo = groupedByInterviewAndRoster.Value.Select(v => v.Value)
                    };
                }
            }
        }
    }
}
