using System.Collections.Generic;

namespace WB.Services.Export.InterviewDataStorage.InterviewDataExport
{
    public class UpdateValueForTableRowInfo
    {
        public UpdateValueForTableRowInfo(string tableName, RosterTableKey rosterLevelTableKey, 
            IEnumerable<UpdateValueInfo> updateValuesInfo)
        {
            TableName = tableName;
            RosterLevelTableKey = rosterLevelTableKey;
            UpdateValuesInfo = updateValuesInfo;
        }

        public string TableName { get; set; }
        public RosterTableKey RosterLevelTableKey { get; set; }
        public IEnumerable<UpdateValueInfo> UpdateValuesInfo { get; set; }
    }
}
