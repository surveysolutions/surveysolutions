using System;
using System.Collections.Generic;

namespace ApiUtil.StatusChange
{
    public class ProcessStatus
    {
        public ProcessStatus()
        {
            this.Errors = new List<InterviewStatusChangeError>();
        }

        public string InterviewImportProcessId { get; set; }
        public bool IsInProgress { get; set; } = false;
        public DateTime StartedDateTime { get; set; }
        public int TotalInterviewsCount { get; set; }
        public int ProcessedInterviewsCount { get; set; }
        public double TimePerInterview { get; set; }
        public double ElapsedTime { get; set; }
        public double EstimatedTime { get; set; }
        public List<InterviewStatusChangeError> Errors { get; set; }

        public override string ToString()
        {
            var finishTime = new TimeSpan((long)(this.EstimatedTime - this.ElapsedTime));
            var result = $"{this.ProcessedInterviewsCount}/{this.TotalInterviewsCount}. ";
            result += $"OK: {this.ProcessedInterviewsCount - this.Errors.Count} Errors: {this.Errors.Count}. ";
            result += $"End in: {finishTime:g}.";
            return result;
        }
    }
}