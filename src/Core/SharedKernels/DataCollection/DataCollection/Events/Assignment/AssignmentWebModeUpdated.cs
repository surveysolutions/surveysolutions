using System;

namespace WB.Core.SharedKernels.DataCollection.Events.Assignment
{
    public class AssignmentWebModeUpdated : AssignmentEvent
    {
        public bool WebMode { get; }

        public AssignmentWebModeUpdated(Guid userId, DateTimeOffset originDate, bool webMode) : base(userId, originDate)
        {
            WebMode = webMode;
        }
    }
}
