using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class InterviewStatisticsReportRow : IView
    {
        public virtual int Id { get; set; }
        public virtual int InterviewId { get; set; }
        public virtual int EntityId { get; set; }
        public virtual string RosterVector { get; set; }

        public virtual long[] Answer { get; set; }
        public virtual StatisticsReportType Type { get; set; }
        public virtual bool IsEnabled { get; set; }

        protected bool Equals(InterviewStatisticsReportRow other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((InterviewStatisticsReportRow) obj);
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}
