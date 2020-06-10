using System;
using System.Diagnostics;
using WB.Services.Infrastructure.EventSourcing;

namespace WB.Services.Export.InterviewDataStorage.InterviewDataExport
{
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public class RosterTableKey
    {
        public Guid InterviewId { get; set; }
        public RosterVector RosterVector { get; set; } = RosterVector.Empty;

        public override string ToString() => InterviewId + "-" + RosterVector;

        public override bool Equals(object obj)
        {
            var item = obj as RosterTableKey;
            if (item == null)
                return false;

            return this.InterviewId.Equals(item.InterviewId) && this.RosterVector.Equals(item.RosterVector);
        }

        public override int GetHashCode()
        {
            return this.InterviewId.GetHashCode() ^ this.RosterVector.GetHashCode();
        }
    }
}
