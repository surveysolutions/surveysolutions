using System;
using System.Diagnostics;
using WB.Services.Infrastructure.EventSourcing;

namespace WB.Services.Export.InterviewDataStorage.InterviewDataExport
{
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public class RosterInfo
    {
        public Guid InterviewId { get; set; }
        public RosterVector RosterVector { get; set; }

        public override string ToString() => InterviewId + "-" + RosterVector;

        public override bool Equals(object obj)
        {
            var item = obj as RosterInfo;
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
