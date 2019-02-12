using System.Collections.Generic;

namespace WB.Services.Export.InterviewDataStorage.InterviewDataExport
{
    public class UpdateValueForTableRowInfo
    {
        public string TableName { get; set; }
        public RosterInfo RosterLevelInfo { get; set; }
        public IEnumerable<UpdateValueInfo> UpdateValuesInfo { get; set; }
    }
}