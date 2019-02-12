using System;
using System.Collections.Generic;

namespace WB.Services.Export.InterviewDataStorage.InterviewDataExport
{
    public class InterviewTableInfo
    {
        public string TableName { get; set; }
        public IEnumerable<Guid> InterviewIds { get; set; }
    }
}
