using System;

namespace WB.Core.SharedKernels.DataCollection.Commands.Assignment
{
    public class UpdateAssignmentAudioRecording : AssignmentCommand
    {
        public bool IsAudioRecordingEnabled { get; }

        public UpdateAssignmentAudioRecording(Guid assignmentId, Guid userId, bool isAudioRecordingEnabled) : base(assignmentId, userId)
        {
            IsAudioRecordingEnabled = isAudioRecordingEnabled;
        }
    }
}
