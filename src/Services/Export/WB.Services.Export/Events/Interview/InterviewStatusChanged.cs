using System;
using WB.Services.Export.Events.Interview.Base;
using WB.Services.Export.Interview;

namespace WB.Services.Export.Events.Interview
{
    public class InterviewStatusChanged : InterviewPassiveEvent
    {
        public InterviewStatus? PreviousStatus { get; private set; }
        public InterviewStatus Status { get; private set; }
        public string Comment { get; private set; }
        public DateTime? UtcTime { get; set; }

        public InterviewStatusChanged(InterviewStatus status, 
            string comment, 
            DateTimeOffset originDate, 
            InterviewStatus? previousStatus = null)
        {
            this.PreviousStatus = previousStatus;
            this.Status = status;
            this.Comment = comment;
        }
    }

    public class InterviewKeyAssigned : InterviewPassiveEvent
    {
        public InterviewKey Key { get; set; } = null!;
    }

    public class InterviewKey
    {
        public int RawValue { get; }

        public InterviewKey(int rawValue)
        {
            this.RawValue = rawValue;
        }

        public override string ToString()
        {
            return this.RawValue.ToString("00-00-00-00");
        }

        protected bool Equals(InterviewKey other)
        {
            return this.RawValue == other.RawValue;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((InterviewKey)obj);
        }

        public override int GetHashCode()
        {
            return this.RawValue;
        }
    }

}
