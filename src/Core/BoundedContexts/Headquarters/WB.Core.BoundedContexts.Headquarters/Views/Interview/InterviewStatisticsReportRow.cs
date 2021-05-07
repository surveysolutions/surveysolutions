using WB.Core.Infrastructure.Domain;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class InterviewStatisticsReportRow : EntityBase<int>, IView
    {
        public virtual int EntityId { get; set; }
        public virtual string RosterVector { get; set; }

        public virtual long[] Answer { get; set; }
        public virtual StatisticsReportType Type { get; set; }
        public virtual bool IsEnabled { get; set; }
        public virtual InterviewSummary InterviewSummary { get; set; }

        protected override bool SignatureEquals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return SignatureEquals((InterviewStatisticsReportRow) obj);
        }

        protected bool SignatureEquals(InterviewStatisticsReportRow other)
        {
            return base.Equals(other) && EntityId == other.EntityId && string.Equals(RosterVector, other.RosterVector);
        }

        protected override int GetSignatureHashCode()
        {
            unchecked
            {
                int hashCode = BaseGetHashCode();
                hashCode = (hashCode * 397) ^ EntityId;
                hashCode = (hashCode * 397) ^ (RosterVector?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}
