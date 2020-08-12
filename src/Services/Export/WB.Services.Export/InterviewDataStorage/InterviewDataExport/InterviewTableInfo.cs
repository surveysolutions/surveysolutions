using System;
using System.Collections.Generic;

namespace WB.Services.Export.InterviewDataStorage.InterviewDataExport
{
    public class InterviewTableInfo
    {
        public InterviewTableInfo(string tableName, IEnumerable<Guid> interviewIds)
        {
            TableName = tableName;
            InterviewIds = interviewIds;
        }

        public string TableName { get; set; }
        public IEnumerable<Guid> InterviewIds { get; set; }
    }
}
