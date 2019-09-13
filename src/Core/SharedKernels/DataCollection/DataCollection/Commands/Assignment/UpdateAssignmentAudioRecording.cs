using System;

namespace WB.Core.SharedKernels.DataCollection.Commands.Assignment
{
    public class UpdateAssignmentAudioRecording : AssignmentCommand
    {
        public bool AudioRecording { get; }

        public UpdateAssignmentAudioRecording(Guid assignmentId, Guid userId, bool audioRecording) : base(assignmentId, userId)
        {
            AudioRecording = audioRecording;
        }
    }
}
