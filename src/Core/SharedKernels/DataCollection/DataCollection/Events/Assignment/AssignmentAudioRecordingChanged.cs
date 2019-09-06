using System;

namespace WB.Core.SharedKernels.DataCollection.Events.Assignment
{
    public class AssignmentAudioRecordingChanged : AssignmentEvent
    {
        public bool IsAudioRecordingEnabled { get; }

        public AssignmentAudioRecordingChanged(Guid userId, DateTimeOffset originDate, bool isAudioRecordingEnabled) 
            : base(userId, originDate)
        {
            IsAudioRecordingEnabled = isAudioRecordingEnabled;
        }
    }
}
