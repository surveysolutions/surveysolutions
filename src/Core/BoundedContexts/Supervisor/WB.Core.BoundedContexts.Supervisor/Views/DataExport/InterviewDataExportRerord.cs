using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Supervisor.Views.DataExport
{
    public class InterviewDataExportRerord
    {
        public InterviewDataExportRerord(Guid interviewId, int recordId, decimal? parentLevelId, Dictionary<Guid, ExportedQuestion> questions)
        {
            InterviewId = interviewId;
            RecordId = recordId;
            ParentRecordId = parentLevelId;
            Questions = questions;
        }

        public Guid InterviewId { get; private set; }
        public int RecordId { get; private set; }
        public decimal? ParentRecordId { get; private set; }
        public Dictionary<Guid,ExportedQuestion> Questions { get; private set; }
    }
}
