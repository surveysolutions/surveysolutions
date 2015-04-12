using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;

namespace WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory
{
    public class InterviewHistoricalRecordView
    {
        public InterviewHistoricalRecordView(long index, InterviewHistoricalAction action, string originatorName, string originatorRole, Dictionary<string, string> parameters, DateTime? timestamp)
        {
            this.Index = index;
            this.Action = action;
            this.OriginatorName = originatorName;
            this.OriginatorRole = originatorRole;
            this.Parameters = parameters;
            this.Timestamp = timestamp;
        }
        public long  Index { get; private set; }
        public InterviewHistoricalAction Action { get; private set; }
        public string OriginatorName { get; private set; }
        public string OriginatorRole { get; private set; }
        public Dictionary<string, string> Parameters { get; private set; }
        public DateTime? Timestamp { get; private set; }
    }
}
