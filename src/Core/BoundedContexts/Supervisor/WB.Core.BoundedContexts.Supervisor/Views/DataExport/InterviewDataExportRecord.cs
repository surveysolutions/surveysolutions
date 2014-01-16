using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.Infrastructure.ReadSide;

namespace WB.Core.BoundedContexts.Supervisor.Views.DataExport
{
    public class InterviewDataExportRecord 
    {
        public InterviewDataExportRecord(Guid interviewId, decimal recordId, decimal? parentLevelId,
            ExportedQuestion[] questions)
        {
            InterviewId = interviewId;
            RecordId = recordId;
            ParentRecordId = parentLevelId;
            Questions = questions;
        }

        public Guid InterviewId { get; private set; }
        public decimal RecordId { get; private set; }
        public decimal? ParentRecordId { get; private set; }
        public ExportedQuestion[] Questions { get; private set; }
    }
}
