using System;

namespace WB.Core.SharedKernels.DataCollection.Events.Assignment
{
    public class AssignmentWebModeChanged : AssignmentEvent
    {
        public bool WebMode { get; }

        public AssignmentWebModeChanged(Guid userId, DateTimeOffset originDate, bool webMode) : base(userId, originDate)
        {
            WebMode = webMode;
        }
    }
}
