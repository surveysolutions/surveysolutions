using System.Collections.Generic;

namespace WB.Services.Export.InterviewDataStorage.InterviewDataExport
{
    public class UpdateValueForTableRowInfo
    {
        public string TableName { get; set; }
        public RosterTableKey RosterLevelTableKey { get; set; }
        public IEnumerable<UpdateValueInfo> UpdateValuesInfo { get; set; }
    }
}
