#nullable enable
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection
{
    public static class EventHelpers
    {
        public static InterviewModeChanged InterviewModeChanged(this InterviewCommand command, InterviewMode mode, string? comment = null)
        {
            return new InterviewModeChanged(command.UserId, command.OriginDate, mode, comment);
        }
    }
}
